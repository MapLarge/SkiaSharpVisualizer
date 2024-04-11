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

		private async Task OnStateChangedAsync(object? sender, VisualizerTargetStateNotification args) {
			var dataSource = await GetRequestAsync(args);

			//This is where we'd delete the previous image if WPF/VS didn't lock it.
			var prevFilePath = this.FilePath;
			this.FilePath = null;

			//There seems to be a bug with the data template and using a BitmapSource from the data context, so we will write the png to a temp file since binding to a url works.
			var tmpFilePath = System.IO.Path.ChangeExtension(System.IO.Path.GetTempFileName(), "png");
			var pngBase64 = dataSource?.pngBase64;
			var pngBytes = !string.IsNullOrWhiteSpace(pngBase64) ? System.Convert.FromBase64String(pngBase64) : Array.Empty<byte>();
			await System.IO.File.WriteAllBytesAsync(tmpFilePath, pngBytes);

			this.FilePath = tmpFilePath;
			this.Model = dataSource;
		}
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

		public void Dispose() {
			visualizerTarget.StateChanged -= this.OnStateChangedAsync;
			this.visualizerTarget.Dispose();

			//This is where we'd delete the previous image if WPF/VS didn't lock it.
			var prevFilePath = this.FilePath;
			this.FilePath = null;
		}

	}
}
