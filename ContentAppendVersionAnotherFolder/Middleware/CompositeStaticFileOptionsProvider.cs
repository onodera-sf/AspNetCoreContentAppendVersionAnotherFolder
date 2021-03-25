using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Extensions.FileProviders
{
  /// <summary>
  /// wwwroot フォルダ以外のファイルで "asp-append-version”を有効にするための複数の <see cref="StaticFileOptions"/> を管理するファイルプロバイダです。
  /// </summary>
  class CompositeStaticFileOptionsProvider : IFileProvider
  {
    private readonly IFileProvider _webRootFileProvider;
    private readonly IEnumerable<StaticFileOptions> _staticFileOptions;

    /// <summary>
    /// コンストラクタです。
    /// </summary>
    /// <param name="webRootFileProvider">
    /// デフォルトの wwwroot が設定されている WebRootFileProvider を指定します。通常は env.WebRootFileProvider を指定してください。
    /// これは追加した <see cref="StaticFileOptions"/> がヒットしなかった場合に使用するためです。
    /// </param>
    /// <param name="staticFileOptions">
    /// 追加した静的ファイルオプションの一覧です。
    /// FileProvider と RequestPath が設定されている必要があります。
    /// </param>
    public CompositeStaticFileOptionsProvider(IFileProvider webRootFileProvider, IEnumerable<StaticFileOptions> staticFileOptions)
    {
      _webRootFileProvider = webRootFileProvider ?? throw new ArgumentNullException(nameof(webRootFileProvider));
      _staticFileOptions = staticFileOptions;
    }

    /// <summary>
    /// 指定されたパスにあるディレクトリを列挙します（存在する場合）。
    /// </summary>
    /// <param name="subpath">ディレクトリを識別する相対パス。</param>
    /// <returns>ディレクトリの内容を返します。</returns>
    public IDirectoryContents GetDirectoryContents(string subpath)
    {
      var result = GetFileProvider(subpath);
      return result.FileProvider.GetDirectoryContents(result.StaticFileRelativePath);
    }

    /// <summary>
    /// 指定されたパスでファイルを見つけます。
    /// </summary>
    /// <param name="subpath">ファイルを識別する相対パス。</param>
    /// <returns>ファイル情報。 発信者はExistsプロパティを確認する必要があります。</returns>
    public IFileInfo GetFileInfo(string subpath)
    {
      var result = GetFileProvider(subpath);
      return result.FileProvider.GetFileInfo(result.StaticFileRelativePath);
    }

    /// <summary>
    /// 指定されたフィルターの Microsoft.Extensions.Primitives.IChangeToken を作成します。
    /// </summary>
    /// <param name="filter">監視するファイルまたはフォルダーを決定するために使用されるフィルター文字列。 例：**/*.cs、*.*、subFolder/**/*.cshtml。</param>
    /// <returns>ファイル一致フィルターが追加、変更、または削除されたときに通知される Microsoft.Extensions.Primitives.IChangeToken。</returns>
    public IChangeToken Watch(string filter)
    {
      var result = GetFileProvider(filter);
      return result.FileProvider.Watch(result.StaticFileRelativePath);
    }

    /// <summary>
    /// 指定された相対 URL に含まれる <see cref="StaticFileOptions"/> を探し、その FileProvider と静的ファイルへの相対パスを返します。
    /// 見つからなかった場合は wwwroot を持つ FileProvider を返します。
    /// </summary>
    /// <param name="path">アクセスされたホスト名以降の相対 URL。</param>
    /// <returns>検索された <see cref="StaticFileOptions"/> の FileProvider と RequestPath から静的ファイルへの相対パス。</returns>
    private (IFileProvider FileProvider, string StaticFileRelativePath) GetFileProvider(string path)
    {
      if (_staticFileOptions != null)
      {
        foreach (var option in _staticFileOptions)
        {
          // 登録している RequestPath とアクセスされた URL の大文字小文字が異なる場合があるので OrdinalIgnoreCase を指定
          if (path.StartsWith(option.RequestPath, StringComparison.OrdinalIgnoreCase))
          {
            return (option.FileProvider, path[option.RequestPath.Value.Length..]);
          }
        }
      }
      return (_webRootFileProvider, path);
    }
  }
}