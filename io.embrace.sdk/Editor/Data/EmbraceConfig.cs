namespace EmbraceSDK.EditorView
{
	public abstract class EmbraceAppConfig
	{
		public abstract string AppIdKey { get; }
		public abstract string SymbolUploadTokenKey { get; }

		public string AppId
		{
			get => EmbraceProjectSettings.Project.GetValue<string>(AppIdKey, string.Empty);
			set => EmbraceProjectSettings.Project.SetValue<string>(AppIdKey, value);
		}

		public string SymbolUploadApiToken {
			get => EmbraceProjectSettings.Project.GetValue<string>(SymbolUploadTokenKey, string.Empty);
			set => EmbraceProjectSettings.Project.SetValue<string>(SymbolUploadTokenKey, value);
		}
	}
}
