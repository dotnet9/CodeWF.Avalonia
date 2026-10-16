# 原生卸载样式隔离验证

需要 Windows、PowerShell 7、Python 3（仅标准库）和 Inno Setup 6 编译器。生成器只编译；runner 仅安装、卸载新建临时目录中的专用夹具。无需管理员权限，不创建卸载注册表、快捷方式、关联或启动产品。

先构建一次，`--output` 使用临时目录。以下 `$fixture`、`$compiler` 和 `$python` 由执行环境提供，路径支持空格：

```powershell
& $python scripts/installer/tests/build_uninstall_fixture.py --iscc $compiler --output $fixture
if ($LASTEXITCODE -ne 0) { throw '夹具构建失败' }

& scripts/installer/tests/verify_uninstall_fixture.ps1 -FixtureDirectory $fixture -Language english
& scripts/installer/tests/verify_uninstall_fixture.ps1 -FixtureDirectory $fixture -Language chinesesimplified
& scripts/installer/tests/verify_uninstall_fixture.ps1 -FixtureDirectory $fixture -Language english -Silent
& scripts/installer/tests/verify_uninstall_fixture.ps1 -FixtureDirectory $fixture -Language english -MissingAssets
```

四个模式必须串行运行；runner 用互斥量拒绝同时运行。CI 应在可创建 Win32 桌面的 Windows 环境中执行，并让任一异常终止当前门禁。生成器也支持 `INNO_ISCC` 或 PATH 查找编译器；省略 `--output` 时自动新建临时目录。不要把构建目录或截图提交进仓库。

每轮会安装 1200 个虚拟文本文件及六个位图，在独立 Win32 桌面执行该轮目录内的 `unins000.exe`。只对该进程树操作精确 HWND；退出时只关闭持有的进程句柄，不按名称终止进程。额外写入的未注册用户文件必须保留，原生卸载日志登记的夹具文件必须移除。

验收覆盖中英文、深→浅→system、动态 Hint、只读系统外观、注入 `WM_SETTINGCHANGE`、signed LPARAM 边界、GDI 稳定性与资源释放、静默跳过和缺资源回退。中英文各 75 项，静默与缺资源各 6 项，共 162 项；另检查原生卸载进程退出码为 0。

进度证据来自夹具删除虚拟文件时的原生 PBM 消息。观察钩子先转发消息，再查询 HWND 的范围和位置；不设置进度、不使用计时模拟。绘制百分比必须等于原生 HWND 比例，Inno 包装属性因二次缩放允许一个百分点的舍入差异。这些结果不能作为真实产品卸载、升级、UAC、多 DPI 或辅助功能读取器已验证的证据。系统态测试不修改系统偏好，因此不会强制覆盖注册表的两种值。

`build-receipt.json` 绑定源码、观察钩子、生成器、生产 include、编译器和二进制 SHA256。runner 在启动前核验来源及安全标记；每轮 `run-*` 目录保存 `receipt.json`、原生日志、观察 JSON 和截图。初始化事件中的截图早于原生子控件完整显示，视觉检查应使用 `real-progress-middle/light/system.png`。
