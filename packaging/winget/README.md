# winget Packaging Notes

`JekyllNet` 在仓库里已经准备好了 `winget` 提交模板，但真正提交到社区源之前，还需要一个已发布的 Windows 可下载资产。

推荐的发布物：

- `JekyllNet-win-x64.zip`
- 内部包含 `jekyllnet.exe`

仓库里现成可用的辅助内容：

- `.github/workflows/release-artifacts.yml`
- `scripts/Export-WingetManifest.ps1`
- `packaging/winget/templates/JekyllNet.JekyllNet.yaml`
- `packaging/winget/templates/JekyllNet.JekyllNet.installer.yaml`
- `packaging/winget/templates/JekyllNet.JekyllNet.locale.en-US.yaml`

## 建议流程

1. 推送 `vX.Y.Z` tag，或手动触发 `release-artifacts` workflow
2. workflow 生成 `JekyllNet-win-x64.zip`、`SHA256SUMS.txt` 和 `artifacts/winget/JekyllNet.JekyllNet/<version>/`
3. 若是 tag 触发，workflow 会把 zip、checksum 和生成后的 manifests 一并挂到 GitHub Release
4. 用 `winget validate` 或 `wingetcreate validate` 验证生成后的 manifests
5. 向 `microsoft/winget-pkgs` 提交 PR

## 本地生成 manifest

```powershell
.\scripts\Export-WingetManifest.ps1 `
  -Version 0.1.0 `
  -InstallerUrl https://github.com/JekyllNet/JekyllNet/releases/download/v0.1.0/JekyllNet-win-x64.zip `
  -ZipPath .\artifacts\JekyllNet-win-x64.zip
```

默认输出目录：

- `artifacts/winget/JekyllNet.JekyllNet/<version>/`

## 需要替换的占位符

- `__VERSION__`
- `__WINDOWS_X64_ZIP_URL__`
- `__WINDOWS_X64_ZIP_SHA256__`

## 说明

- 当前模板按 `portable` 命令行工具分发思路准备
- 命令别名为 `jekyllnet`
- 如果未来改成 MSI 或其他安装器类型，需同步调整 installer manifest
