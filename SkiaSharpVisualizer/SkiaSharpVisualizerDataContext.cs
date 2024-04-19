using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.DebuggerVisualizers;
using Microsoft.VisualStudio.Extensibility.UI;
using Microsoft.VisualStudio.RpcContracts.DebuggerVisualizers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SkiaSharpVisualizer {

	[DataContract]
	public class SkiaSharpVisualizerDataContext : NotifyPropertyChangedObject, IDisposable {

		private readonly VisualizerTarget visualizerTarget;
		private SkiaSharpVisualizerDataSource? _model;

		public SkiaSharpVisualizerDataContext(VisualizerTarget visualizerTarget) {
			this.visualizerTarget = visualizerTarget;
			visualizerTarget.StateChanged += this.OnStateChangedAsync;

			this.OpenExternalCommand = new OpenExternalCommand(this);
		}

		[DataMember]
		public SkiaSharpVisualizerDataSource? Model {
			get => _model;
			set {
				SetProperty(ref this._model, value);
				RaiseNotifyPropertyChangedEvent(nameof(Width));
				RaiseNotifyPropertyChangedEvent(nameof(Height));
			}
		}
		[DataMember]
		public int Width => Model?.width ?? 0;
		[DataMember]
		public int Height => Model?.height ?? 0;

		private string? _filePath;
		[DataMember]
		public string? FilePath {
			get => _filePath;
			set {
				SetProperty(ref _filePath, value);
			}
		}

		private bool _isStretched;
		[DataMember]
		public bool IsStretched {
			get => _isStretched;
			set {
				SetProperty(ref _isStretched, value);
				RaiseNotifyPropertyChangedEvent(nameof(ImageStretch));
			}
		}
		[DataMember]
		public string ImageStretch => _isStretched ? "Uniform" : "None";

		private bool _isBordered;
		[DataMember]
		public bool IsBordered {
			get => _isBordered;
			set {
				SetProperty(ref _isBordered, value);
				RaiseNotifyPropertyChangedEvent(nameof(BorderThickness));
			}
		}
		[DataMember]
		public int BorderThickness => _isBordered ? 3 : 0;

		private const int MAXFILEPATHS = 5;
		private SortedDictionary<string, string> byteFilePaths = new();
		private SortedDictionary<string, DateTimeOffset> byteLastAccess = new();

		[DataMember]
		public IAsyncCommand OpenExternalCommand { get; }

#if DEBUG
		private readonly List<string> failedToDeleteFiles = new();
#endif

		private async Task<SkiaSharpVisualizerDataSource?> GetRequestAsync(VisualizerTargetStateNotification args) {
			switch (args) {
				case VisualizerTargetStateNotification.Available:
				case VisualizerTargetStateNotification.ValueUpdated:
					return await visualizerTarget.ObjectSource.RequestDataAsync<SkiaSharpVisualizerDataSource>(jsonSerializer: null, CancellationToken.None);
				case VisualizerTargetStateNotification.Unavailable:
					return null;
				default:
					throw new NotSupportedException("Unexpected visualizer target state notification");
			}
		}

		private async Task OnStateChangedAsync(object? sender, VisualizerTargetStateNotification args) {
			var dataSource = await GetRequestAsync(args);
			
			CleanupUsedFilePaths();

			//No data.
			var pngBase64 = dataSource?.pngBase64;
			if (string.IsNullOrWhiteSpace(pngBase64)) {
				ResetBindings();
				return;
			}

			var pngBytes = System.Convert.FromBase64String(pngBase64);
			try {
				//Is this the same image we've shown already?
				if (byteFilePaths.TryGetValue(pngBase64, out var fp) && System.IO.File.Exists(fp) && System.IO.File.ReadAllBytes(fp).SequenceEqual(pngBytes)) {
					this.byteLastAccess[pngBase64] = DateTimeOffset.Now;
					this.FilePath = fp;
					this.Model = dataSource;
					return;
				}
			} catch {
				//Ignore any lookup errors.
			}

			//Using a BitmapSource on the data context is not serializable cross-process, so we will write the png to a temp file since binding to a url works.
			try {
				var tmpFilePath = System.IO.Path.ChangeExtension(System.IO.Path.GetTempFileName(), "png");
				await System.IO.File.WriteAllBytesAsync(tmpFilePath, pngBytes);

				this.byteFilePaths[pngBase64] = tmpFilePath;
				this.byteLastAccess[pngBase64] = DateTimeOffset.Now;
				this.FilePath = tmpFilePath;
				this.Model = dataSource;
			} catch {
				//Something terrible happened.
				ResetBindings();
			}
		}

		private void CleanupUsedFilePaths() {
			//Once we hit the max limit, remove the oldest tracked file.
			if (byteLastAccess.Count < MAXFILEPATHS) {
				return;
			}

			var oldestFile = byteLastAccess.First();
			if (!byteFilePaths.TryGetValue(oldestFile.Key, out var filePath)) {
				//Shouldn't happen.
				byteLastAccess.Remove(oldestFile.Key);
				return;
			}

			byteFilePaths.Remove(oldestFile.Key);
			byteLastAccess.Remove(oldestFile.Key);
			try {
				//Make an attempt to remove the file.
				System.IO.File.Delete(filePath);
			} catch {
				//Ignore IO errors
#if DEBUG
				failedToDeleteFiles.Add(filePath);
#endif
			}
		}
		private void ResetBindings() {
			this.FilePath = null;
			this.Model = null;
		}
		private void RemoveAllFiles() {
			foreach (var kvp in byteFilePaths) {
				try {
					//Make an attempt to remove the file. VS locks them for a while, so we might not get them all.
					System.IO.File.Delete(kvp.Value);
				} catch {
					//Ignore IO errors
#if DEBUG
					failedToDeleteFiles.Add(kvp.Value);
#endif
				}
			}

			this.byteFilePaths.Clear();
			this.byteLastAccess.Clear();
		}

		public void Dispose() {
			visualizerTarget.StateChanged -= this.OnStateChangedAsync;
			this.visualizerTarget.Dispose();

			this.ResetBindings();
			this.RemoveAllFiles();
		}

	}

	public class OpenExternalCommand : NotifyPropertyChangedObject, IAsyncCommand {

		private bool executeFailed = false;
		public bool CanExecute => !executeFailed && !string.IsNullOrWhiteSpace(context.FilePath);

		private readonly SkiaSharpVisualizerDataContext context;
		public OpenExternalCommand(SkiaSharpVisualizerDataContext context) {
			this.context = context;
			this.context.PropertyChanged += Context_PropertyChanged;
		}

		private void Context_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
			switch (e.PropertyName) {
				case nameof(context.FilePath):
					this.RaiseNotifyPropertyChangedEvent("CanExecute");
					break;
			}
		}

		public Task ExecuteAsync(object? parameter, IClientContext clientContext, CancellationToken cancellationToken) {
			var filePath = parameter as string;
			if (string.IsNullOrWhiteSpace(filePath)) {
				return Task.CompletedTask;
			}

			try {
				//Need UseShellExecute to run an image file.
				var info = new System.Diagnostics.ProcessStartInfo(filePath);
				info.UseShellExecute = true;
				using var _ = System.Diagnostics.Process.Start(info);
			} catch {
				//Hopefully doesn't happen.
				this.executeFailed = true;
				this.RaiseNotifyPropertyChangedEvent("CanExecute");
			}
			return Task.CompletedTask;
		}
	}

}
