using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Extensions.FileProviders
{
  /// <summary>
  /// wwwroot �t�H���_�ȊO�̃t�@�C���� "asp-append-version�h��L���ɂ��邽�߂̕����� <see cref="StaticFileOptions"/> ���Ǘ�����t�@�C���v���o�C�_�ł��B
  /// </summary>
  class CompositeStaticFileOptionsProvider : IFileProvider
  {
    private readonly IFileProvider _webRootFileProvider;
    private readonly IEnumerable<StaticFileOptions> _staticFileOptions;

    /// <summary>
    /// �R���X�g���N�^�ł��B
    /// </summary>
    /// <param name="webRootFileProvider">
    /// �f�t�H���g�� wwwroot ���ݒ肳��Ă��� WebRootFileProvider ���w�肵�܂��B�ʏ�� env.WebRootFileProvider ���w�肵�Ă��������B
    /// ����͒ǉ����� <see cref="StaticFileOptions"/> ���q�b�g���Ȃ������ꍇ�Ɏg�p���邽�߂ł��B
    /// </param>
    /// <param name="staticFileOptions">
    /// �ǉ������ÓI�t�@�C���I�v�V�����̈ꗗ�ł��B
    /// FileProvider �� RequestPath ���ݒ肳��Ă���K�v������܂��B
    /// </param>
    public CompositeStaticFileOptionsProvider(IFileProvider webRootFileProvider, IEnumerable<StaticFileOptions> staticFileOptions)
    {
      _webRootFileProvider = webRootFileProvider ?? throw new ArgumentNullException(nameof(webRootFileProvider));
      _staticFileOptions = staticFileOptions;
    }

    /// <summary>
    /// �w�肳�ꂽ�p�X�ɂ���f�B���N�g����񋓂��܂��i���݂���ꍇ�j�B
    /// </summary>
    /// <param name="subpath">�f�B���N�g�������ʂ��鑊�΃p�X�B</param>
    /// <returns>�f�B���N�g���̓��e��Ԃ��܂��B</returns>
    public IDirectoryContents GetDirectoryContents(string subpath)
    {
      var result = GetFileProvider(subpath);
      return result.FileProvider.GetDirectoryContents(result.StaticFileRelativePath);
    }

    /// <summary>
    /// �w�肳�ꂽ�p�X�Ńt�@�C���������܂��B
    /// </summary>
    /// <param name="subpath">�t�@�C�������ʂ��鑊�΃p�X�B</param>
    /// <returns>�t�@�C�����B ���M�҂�Exists�v���p�e�B���m�F����K�v������܂��B</returns>
    public IFileInfo GetFileInfo(string subpath)
    {
      var result = GetFileProvider(subpath);
      return result.FileProvider.GetFileInfo(result.StaticFileRelativePath);
    }

    /// <summary>
    /// �w�肳�ꂽ�t�B���^�[�� Microsoft.Extensions.Primitives.IChangeToken ���쐬���܂��B
    /// </summary>
    /// <param name="filter">�Ď�����t�@�C���܂��̓t�H���_�[�����肷�邽�߂Ɏg�p�����t�B���^�[������B ��F**/*.cs�A*.*�AsubFolder/**/*.cshtml�B</param>
    /// <returns>�t�@�C����v�t�B���^�[���ǉ��A�ύX�A�܂��͍폜���ꂽ�Ƃ��ɒʒm����� Microsoft.Extensions.Primitives.IChangeToken�B</returns>
    public IChangeToken Watch(string filter)
    {
      var result = GetFileProvider(filter);
      return result.FileProvider.Watch(result.StaticFileRelativePath);
    }

    /// <summary>
    /// �w�肳�ꂽ���� URL �Ɋ܂܂�� <see cref="StaticFileOptions"/> ��T���A���� FileProvider �ƐÓI�t�@�C���ւ̑��΃p�X��Ԃ��܂��B
    /// ������Ȃ������ꍇ�� wwwroot ������ FileProvider ��Ԃ��܂��B
    /// </summary>
    /// <param name="path">�A�N�Z�X���ꂽ�z�X�g���ȍ~�̑��� URL�B</param>
    /// <returns>�������ꂽ <see cref="StaticFileOptions"/> �� FileProvider �� RequestPath ����ÓI�t�@�C���ւ̑��΃p�X�B</returns>
    private (IFileProvider FileProvider, string StaticFileRelativePath) GetFileProvider(string path)
    {
      if (_staticFileOptions != null)
      {
        foreach (var option in _staticFileOptions)
        {
          // �o�^���Ă��� RequestPath �ƃA�N�Z�X���ꂽ URL �̑啶�����������قȂ�ꍇ������̂� OrdinalIgnoreCase ���w��
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