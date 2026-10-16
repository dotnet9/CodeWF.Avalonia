"""生成并编译独立卸载夹具；只使用 Python 标准库，不运行任何安装器。"""

import argparse
import hashlib
import json
import os
from pathlib import Path
import re
import shutil
import subprocess
import tempfile


HERE = Path(__file__).resolve().parent
APP_ID = "CodeWF.Uninstall.Branding.Fixture.IsolatedOnly"
BASE_NAME = "CodeWF.Uninstall.Branding.Fixture"


def require(condition, message):
    if not condition:
        raise RuntimeError(message)


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def script_path(path):
    value = path.resolve().as_posix()
    require(not any(char in value for char in ('"', '\r', '\n', '{', '}')),
            f"夹具路径包含 Inno 保留字符：{path}")
    return value


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--iscc", default=os.environ.get("INNO_ISCC") or shutil.which("ISCC.exe"))
    parser.add_argument("--installer-root", type=Path, default=HERE.parent)
    parser.add_argument("--output", type=Path)
    args = parser.parse_args()
    require(args.iscc, "请传 --iscc，或设置 INNO_ISCC / PATH。")
    compiler = Path(args.iscc).resolve()
    require(compiler.is_file(), "Inno 编译器不存在。")
    installer_root = args.installer_root.resolve()
    include = installer_root / "CodeWF.Setup.Uninstall.iss"
    observer = HERE / "Fixture.Uninstall.iss"
    language = installer_root.parent / "Languages/ChineseSimplified.isl"
    for path in (include, observer, language):
        require(path.is_file(), f"缺少夹具依赖：{path}")
    include_sha = digest(include)
    include_text = include.read_text(encoding="utf-8-sig")
    require(not re.search(r"(?im)^\[(Registry|Icons|Run|UninstallRun|UninstallDelete)\]", include_text),
            "卸载样式 include 不得新增注册表、启动或批量删除指令。")
    output = (args.output or Path(tempfile.mkdtemp(prefix="codewf-uninstall-fixture-"))).resolve()
    output.mkdir(parents=True, exist_ok=True)
    payload = output / "payload"
    payload.mkdir(exist_ok=True)
    for index in range(1200):
        (payload / f"fixture-{index:04}.txt").write_text(
            f"CodeWF isolated uninstall fixture {index}\n", encoding="utf-8")
    source = output / (BASE_NAME + ".iss")
    binary = output / (BASE_NAME + ".exe")
    receipt = output / "build-receipt.json"
    # 禁止前轮二进制或收据掩盖本轮编译失败；只移除精确的夹具产物。
    binary.unlink(missing_ok=True)
    receipt.unlink(missing_ok=True)
    source.write_text(f'''#define CodeWFSetupFixture
#define CodeWFSetupUninstallFixture
#define CodeWFSetupAssetPath "{script_path(installer_root)}"
[Setup]
AppId={APP_ID}
AppName=CodeWF Uninstall Branding Fixture
AppVersion=1.0.0
DefaultDirName={script_path(output)}/default-isolated-install
PrivilegesRequired=lowest
Uninstallable=yes
CreateUninstallRegKey=no
DisableDirPage=yes
DisableProgramGroupPage=yes
DisableReadyPage=yes
DisableFinishedPage=yes
OutputDir={script_path(output)}
OutputBaseFilename={BASE_NAME}
Compression=lzma
SolidCompression=yes
[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "chinesesimplified"; MessagesFile: "{script_path(language)}"
[Files]
Source: "{script_path(payload)}/*"; DestDir: "{{app}}/fixture-owned"; Flags: ignoreversion
#include "{script_path(include)}"
#include "{script_path(observer)}"
''', encoding="utf-8-sig")
    result = subprocess.run([str(compiler), str(source)], capture_output=True, timeout=60)
    log = output / "compile.log"
    log.write_bytes(result.stdout + result.stderr)
    require(result.returncode == 0 and binary.is_file(), f"夹具编译失败，见 {log}")
    require(digest(include) == include_sha, "编译期间生产 include 发生变化，请重新构建。")
    data = {
        "app_id": APP_ID,
        "fixture_directory": str(output),
        "source": str(source), "source_sha256": digest(source),
        "binary": str(binary), "binary_sha256": digest(binary),
        "include": str(include), "include_sha256": include_sha,
        "observer": str(observer), "observer_sha256": digest(observer),
        "generator_sha256": digest(Path(__file__).resolve()),
        "compiler": str(compiler), "compiler_sha256": digest(compiler),
        "compile_log_sha256": digest(log), "compile_exit_code": result.returncode,
        "executed": False,
        "scope": "Only a temporary dummy-file fixture; no product installer/uninstaller or registration.",
    }
    receipt.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"PASS: isolated uninstall fixture compiled: {output}")
    print(f"Include SHA256: {include_sha}")


if __name__ == "__main__":
    main()
