using AnimatedImage.Avalonia;
using Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Controls.Metadata;
using global::Avalonia.Controls.Primitives;
using global::Avalonia.Input;
using global::Avalonia.Interactivity;
using global::Avalonia.Layout;
using global::Avalonia.Media;
using global::Avalonia.Media.Imaging;
using global::Avalonia.Threading;
using CodeWF.Avalonia.Lang;
using SkiaSharp;

using CodeWF.Avalonia.Markdown.Rendering;

namespace CodeWF.Avalonia.Markdown.Controls;

[TemplatePart(ContentHostPartName, typeof(ContentControl), IsRequired = true)]
public class MarkdownImage : TemplatedControl, CodeWF.Avalonia.Markdown.Rendering.IMarkdownInteractiveVisual
{
    private const string ContentHostPartName = "PART_ContentHost";
    private const double DefaultMaxImageWidth = 900;
    private const double DefaultMaxImageHeight = 520;
    private const long MaxImageByteCacheBytes = 128L * 1024 * 1024;
    private const long MaxImagePreviewCacheBytes = 64L * 1024 * 1024;

    private static readonly Dictionary<string, MarkdownImageSource> ImageByteCache = new(StringComparer.Ordinal);
    private static readonly LinkedList<string> ImageByteCacheOrder = new();
    private static readonly object ImageByteCacheGate = new();
    private static long _imageByteCacheBytes;

    private static readonly Dictionary<string, byte[]> ImagePreviewCache = new(StringComparer.Ordinal);
    private static readonly LinkedList<string> ImagePreviewCacheOrder = new();
    private static readonly object ImagePreviewCacheGate = new();
    private static long _imagePreviewCacheBytes;

    private ContentControl? _contentHost;
    private Bitmap? _bitmap;
    private MemoryStream? _animatedStream;
    private byte[]? _imageBytes;
    private byte[]? _previewBytes;
    private string? _fileName;
    private bool _isSvg;
    private bool _isGif;
    private bool _isAnimated;
    private long _loadVersion;
    private CancellationTokenSource? _loadCts;
    private Point? _pressedPoint;
    private bool _isPointerDragging;

    public static readonly StyledProperty<string?> SourceProperty =
        AvaloniaProperty.Register<MarkdownImage, string?>(nameof(Source));

    public static readonly StyledProperty<string?> AltTextProperty =
        AvaloniaProperty.Register<MarkdownImage, string?>(nameof(AltText));

    public static readonly StyledProperty<string?> ImageBasePathProperty =
        AvaloniaProperty.Register<MarkdownImage, string?>(nameof(ImageBasePath));

    public string? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    public string? AltText
    {
        get => GetValue(AltTextProperty);
        set => SetValue(AltTextProperty, value);
    }

    public string? ImageBasePath
    {
        get => GetValue(ImageBasePathProperty);
        set => SetValue(ImageBasePathProperty, value);
    }

    static MarkdownImage()
    {
        SourceProperty.Changed.AddClassHandler<MarkdownImage>((image, _) => image.QueueLoad());
        ImageBasePathProperty.Changed.AddClassHandler<MarkdownImage>((image, _) => image.QueueLoad());
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _contentHost = e.NameScope.Find<ContentControl>(ContentHostPartName);
        QueueLoad();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        Interlocked.Increment(ref _loadVersion);
        CancelCurrentLoad();
        ClearImageState();
        base.OnDetachedFromVisualTree(e);
    }

    private void QueueLoad()
    {
        var source = Source?.Trim();
        var version = Interlocked.Increment(ref _loadVersion);
        CancelCurrentLoad();

        if (string.IsNullOrWhiteSpace(source))
        {
            TrimImageState();
            SetContent(null);
            return;
        }

        var loadCts = new CancellationTokenSource();
        _loadCts = loadCts;
        _ = LoadAsync(source, version, loadCts);
    }

    private void CancelCurrentLoad()
    {
        var loadCts = _loadCts;
        _loadCts = null;
        loadCts?.Cancel();
    }

    private void ClearImageState()
    {
        var oldBitmap = _bitmap;
        var oldAnimatedStream = _animatedStream;
        _bitmap = null;
        _animatedStream = null;
        _imageBytes = null;
        _previewBytes = null;
        _fileName = null;
        _isSvg = false;
        _isGif = false;
        _isAnimated = false;
        _pressedPoint = null;
        _isPointerDragging = false;

        SetContent(null);
        oldBitmap?.Dispose();
        oldAnimatedStream?.Dispose();
    }

    /// <summary>
    /// 解码结果只保留在静态缓存里；控件被回收/重建时 <see cref="ClearImageState"/> 不应丢弃它，
    /// 否则块重新物化又要从字节重新解码（SVG/GIF 还要重新光栅化），滚动就卡。
    /// </summary>
    private void TrimImageState()
    {
        _bitmap = null;
        _imageBytes = null;
        _fileName = null;
        _pressedPoint = null;
        _isPointerDragging = false;
    }

    private async Task LoadAsync(string source, long version, CancellationTokenSource loadCts)
    {
        var token = loadCts.Token;
        var cacheKey = MarkdownImageSourceLoader.CreateCacheKey(source, ImageBasePath);

        try
        {
            token.ThrowIfCancellationRequested();
            var loadResult = await LoadBytesAsync(source, token);
            token.ThrowIfCancellationRequested();
            var previewBytes = ResolvePreviewBytes(cacheKey, loadResult);
            token.ThrowIfCancellationRequested();
            using var stream = new MemoryStream(previewBytes);
            var bitmap = new Bitmap(stream);
            var fileName = loadResult.FileName;

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (version != _loadVersion || token.IsCancellationRequested)
                {
                    bitmap.Dispose();
                    return;
                }

                MemoryStream? animatedStream = null;
                var isAnimated = loadResult.IsGif && CountGifFrames(loadResult.Bytes) > 1;
                var content = loadResult.IsSvg
                    ? CreateSvgContent(bitmap)
                    : isAnimated
                        ? CreateAnimatedGifContent(loadResult.Bytes, bitmap, out animatedStream)
                        : CreateBitmapContent(bitmap);

                var oldBitmap = _bitmap;
                var oldAnimatedStream = _animatedStream;
                _bitmap = bitmap;
                _animatedStream = animatedStream;
                _imageBytes = loadResult.Bytes;
                _previewBytes = previewBytes;
                _fileName = fileName;
                _isSvg = loadResult.IsSvg;
                _isGif = loadResult.IsGif;
                _isAnimated = isAnimated;

                SetContent(content);
                oldBitmap?.Dispose();
                oldAnimatedStream?.Dispose();
            });
        }
        catch (OperationCanceledException)
        {
        }
        catch (FileNotFoundException ex)
        {
            ;
            await ShowFallbackAsync(
                version,
                string.Format(MarkdownLocalization.GetResource(CodeWF.Avalonia.Markdown.MarkdownL.ImageFileNotFound), ex.FileName ?? source));
        }
        catch
        {
            await ShowFallbackAsync(
                version,
                string.Format(MarkdownLocalization.GetResource(CodeWF.Avalonia.Markdown.MarkdownL.ImageLoadFailed), AltText ?? source));
        }
        finally
        {
            if (ReferenceEquals(_loadCts, loadCts))
            {
                _loadCts = null;
            }

            loadCts.Dispose();
        }
    }

    /// <summary>
    /// 取静态预览字节：SVG/GIF 需要光栅化，结果按缓存 Key 复用，
    /// 让被回收后重新物化的图片块跳过解析与光栅化。
    /// </summary>
    private static byte[] ResolvePreviewBytes(string cacheKey, MarkdownImageSource loadResult)
    {
        if (!loadResult.IsSvg && !loadResult.IsGif)
        {
            return loadResult.Bytes;
        }

        if (TryGetCachedPreviewBytes(cacheKey, out var cached))
        {
            return cached;
        }

        var previewBytes = MarkdownImageRasterizer.RenderToPngBytes(loadResult);
        AddPreviewBytesToCache(cacheKey, previewBytes);
        return previewBytes;
    }

    /// <summary>GIF 只有一帧时按静态图渲染，避免为一张图启动动画时钟。</summary>
    private static int CountGifFrames(byte[] bytes)
    {
        try
        {
            using var sourceData = SKData.CreateCopy(bytes);
            using var codec = SKCodec.Create(sourceData);
            return codec?.FrameCount ?? 1;
        }
        catch
        {
            return 1;
        }
    }

    private async Task<MarkdownImageSource> LoadBytesAsync(string source, CancellationToken token)
    {
        var cacheKey = MarkdownImageSourceLoader.CreateCacheKey(source, ImageBasePath);
        if (TryGetCachedImageBytes(cacheKey, out var cached))
        {
            return cached;
        }

        var result = await MarkdownImageSourceLoader.LoadAsync(source, ImageBasePath, token);
        AddImageBytesToCache(cacheKey, result);
        return result;
    }

    private static bool TryGetCachedImageBytes(string source, out MarkdownImageSource result)
    {
        lock (ImageByteCacheGate)
        {
            if (!ImageByteCache.TryGetValue(source, out result!))
            {
                return false;
            }

            ImageByteCacheOrder.Remove(source);
            ImageByteCacheOrder.AddLast(source);
            return true;
        }
    }

    private static void AddImageBytesToCache(string source, MarkdownImageSource result)
    {
        lock (ImageByteCacheGate)
        {
            if (result.Bytes.LongLength > MaxImageByteCacheBytes)
            {
                return;
            }

            if (ImageByteCache.TryGetValue(source, out var existing))
            {
                _imageByteCacheBytes -= existing.Bytes.LongLength;
                ImageByteCacheOrder.Remove(source);
                ImageByteCache[source] = result;
            }
            else
            {
                ImageByteCache[source] = result;
            }

            ImageByteCacheOrder.AddLast(source);
            _imageByteCacheBytes += result.Bytes.LongLength;
            while (_imageByteCacheBytes > MaxImageByteCacheBytes && ImageByteCacheOrder.First is { } oldest)
            {
                ImageByteCacheOrder.RemoveFirst();
                if (ImageByteCache.Remove(oldest.Value, out var removed))
                {
                    _imageByteCacheBytes -= removed.Bytes.LongLength;
                }
            }
        }
    }

    private static bool TryGetCachedPreviewBytes(string cacheKey, out byte[] previewBytes)
    {
        lock (ImagePreviewCacheGate)
        {
            if (!ImagePreviewCache.TryGetValue(cacheKey, out previewBytes!))
            {
                return false;
            }

            ImagePreviewCacheOrder.Remove(cacheKey);
            ImagePreviewCacheOrder.AddLast(cacheKey);
            return true;
        }
    }

    private static void AddPreviewBytesToCache(string cacheKey, byte[] previewBytes)
    {
        lock (ImagePreviewCacheGate)
        {
            if (previewBytes.LongLength > MaxImagePreviewCacheBytes)
            {
                return;
            }

            if (ImagePreviewCache.TryGetValue(cacheKey, out var existing))
            {
                _imagePreviewCacheBytes -= existing.LongLength;
                ImagePreviewCacheOrder.Remove(cacheKey);
            }

            ImagePreviewCache[cacheKey] = previewBytes;
            ImagePreviewCacheOrder.AddLast(cacheKey);
            _imagePreviewCacheBytes += previewBytes.LongLength;
            while (_imagePreviewCacheBytes > MaxImagePreviewCacheBytes && ImagePreviewCacheOrder.First is { } oldest)
            {
                ImagePreviewCacheOrder.RemoveFirst();
                if (ImagePreviewCache.Remove(oldest.Value, out var removed))
                {
                    _imagePreviewCacheBytes -= removed.LongLength;
                }
            }
        }
    }

    private void OnImagePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed)
        {
            return;
        }

        _pressedPoint = e.GetPosition(this);
        _isPointerDragging = false;
    }

    private void OnImagePointerMoved(object? sender, PointerEventArgs e)
    {
        if (_pressedPoint is not { } pressedPoint)
        {
            return;
        }

        var currentPoint = e.GetPosition(this);
        if (Math.Abs(currentPoint.X - pressedPoint.X) > 4 || Math.Abs(currentPoint.Y - pressedPoint.Y) > 4)
        {
            _isPointerDragging = true;
        }
    }

    private void OnImagePointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var shouldOpen = e.InitialPressMouseButton == MouseButton.Left && !_isPointerDragging;
        _pressedPoint = null;
        _isPointerDragging = false;

        if (shouldOpen)
        {
            OpenPreviewWindow();
            e.Handled = true;
        }
    }

    private void OpenPreviewWindow()
    {
        if (_bitmap is null || _previewBytes is null)
        {
            return;
        }

        Bitmap previewBitmap;
        try
        {
            using var previewStream = new MemoryStream(_previewBytes);
            previewBitmap = new Bitmap(previewStream);
        }
        catch
        {
            return;
        }

        var title = !string.IsNullOrWhiteSpace(AltText) ? AltText : Source;
        var window = new MarkdownImagePreviewWindow(
            previewBitmap,
            _imageBytes ?? _previewBytes,
            _fileName ?? (_isSvg ? "markdown-image.svg" : _isGif ? "markdown-image.gif" : "markdown-image.png"),
            title,
            _isSvg,
            _isAnimated);
        if (TopLevel.GetTopLevel(this) is Window owner)
        {
            window.Show(owner);
        }
        else
        {
            window.Show();
        }
    }

    private Control CreateBitmapContent(Bitmap bitmap)
    {
        var image = new Image
        {
            Source = bitmap,
            Stretch = Stretch.Uniform,
            HorizontalAlignment = HorizontalAlignment.Left,
            Cursor = new Cursor(StandardCursorType.Hand)
        };
        ApplyImageChrome(image, bitmap);
        AttachImageClick(image);
        return image;
    }

    private Control CreateSvgContent(Bitmap previewBitmap)
    {
        return CreateBitmapContent(previewBitmap);
    }

    private Control CreateAnimatedGifContent(byte[] gifBytes, Bitmap previewBitmap, out MemoryStream animatedStream)
    {
        animatedStream = new MemoryStream(gifBytes, writable: false);
        var image = new Image
        {
            Stretch = Stretch.Uniform,
            HorizontalAlignment = HorizontalAlignment.Left,
            Cursor = new Cursor(StandardCursorType.Hand)
        };
        ImageBehavior.SetAnimatedSource(image, new AnimatedImageSourceStream(animatedStream));
        ImageBehavior.SetRepeatBehavior(image, RepeatBehavior.Forever);
        ApplyImageChrome(image, previewBitmap);
        AttachImageClick(image);
        return image;
    }

    private void ApplyImageChrome(Control control, Bitmap bitmap)
    {
        control.Classes.Add(MarkdownStyleKeys.ImageContent);
        control.MaxWidth = DefaultMaxImageWidth;
        control.MaxHeight = DefaultMaxImageHeight;
        control.Margin = new Thickness(0, 6, 0, 10);

        var (width, height) = CalculateDisplaySize(bitmap.PixelSize.Width, bitmap.PixelSize.Height);
        control.Width = width;
        control.Height = height;
    }

    private (double Width, double Height) CalculateDisplaySize(double imageWidth, double imageHeight)
    {
        imageWidth = Math.Max(1, imageWidth);
        imageHeight = Math.Max(1, imageHeight);

        if (!double.IsNaN(Width) && Width > 0 && !double.IsNaN(Height) && Height > 0)
        {
            var scale = Math.Min(Width / imageWidth, Height / imageHeight);
            return (imageWidth * scale, imageHeight * scale);
        }

        if (!double.IsNaN(Width) && Width > 0)
        {
            var scale = Width / imageWidth;
            return (Width, imageHeight * scale);
        }

        if (!double.IsNaN(Height) && Height > 0)
        {
            var scale = Height / imageHeight;
            return (imageWidth * scale, Height);
        }

        var maxWidth = double.IsNaN(MaxWidth) || double.IsInfinity(MaxWidth) || MaxWidth <= 0
            ? DefaultMaxImageWidth
            : MaxWidth;
        var maxHeight = double.IsNaN(MaxHeight) || double.IsInfinity(MaxHeight) || MaxHeight <= 0
            ? DefaultMaxImageHeight
            : MaxHeight;
        var maxScale = Math.Min(1, Math.Min(maxWidth / imageWidth, maxHeight / imageHeight));
        return (imageWidth * maxScale, imageHeight * maxScale);
    }

    private void AttachImageClick(Control control)
    {
        control.AddHandler(
            InputElement.PointerPressedEvent,
            OnImagePointerPressed,
            RoutingStrategies.Bubble);
        control.AddHandler(
            InputElement.PointerMovedEvent,
            OnImagePointerMoved,
            RoutingStrategies.Bubble);
        control.AddHandler(
            InputElement.PointerReleasedEvent,
            OnImagePointerReleased,
            RoutingStrategies.Bubble);
    }

    private async Task ShowFallbackAsync(long version, string text)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (version != _loadVersion)
            {
                return;
            }

            var oldBitmap = _bitmap;
            var oldAnimatedStream = _animatedStream;
            _bitmap = null;
            _animatedStream = null;
            _imageBytes = null;
            _previewBytes = null;
            _fileName = null;
            _isSvg = false;
            _isGif = false;
            _isAnimated = false;

            var fallback = new Border
            {
                Child = new TextBlock
                {
                    Text = text,
                    TextWrapping = TextWrapping.Wrap
                }
            };
            fallback.Classes.Add(MarkdownStyleKeys.ImageFallback);
            if (fallback.Child is TextBlock textBlock)
            {
                textBlock.Classes.Add(MarkdownStyleKeys.ImageFallbackText);
            }

            SetContent(fallback);
            oldBitmap?.Dispose();
            oldAnimatedStream?.Dispose();
        });
    }

    private void SetContent(Control? content)
    {
        if (_contentHost is not null)
        {
            _contentHost.Content = content;
            _contentHost.InvalidateMeasure();
        }

        InvalidateMeasure();
        InvalidateArrange();
    }
}
