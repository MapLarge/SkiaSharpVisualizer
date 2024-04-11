using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.DebuggerVisualizers;
using Microsoft.VisualStudio.Extensibility.UI;
using Microsoft.VisualStudio.RpcContracts.DebuggerVisualizers;
using Microsoft.VisualStudio.RpcContracts.RemoteUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SkiaSharpVisualizer {

	[VisualStudioContribution]
	public class SkiaSharpVisualizerProvider : DebuggerVisualizerProvider {

		public SkiaSharpVisualizerProvider(SkiaSharpVisualizerExtension extension, VisualStudioExtensibility extensibility) : base(extension, extensibility) {

		}

		/// <inheritdoc/>
		public override DebuggerVisualizerProviderConfiguration DebuggerVisualizerProviderConfiguration => new DebuggerVisualizerProviderConfiguration(
			new VisualizerTargetType("%SkiaSharpVisualizer.SkiaSharpVisualizerProvider.SKBitmap.DisplayName%", typeof(SkiaSharp.SKBitmap)),
			new VisualizerTargetType("%SkiaSharpVisualizer.SkiaSharpVisualizerProvider.SKImage.DisplayName%", typeof(SkiaSharp.SKImage)),
			new VisualizerTargetType("%SkiaSharpVisualizer.SkiaSharpVisualizerProvider.SKSurface.DisplayName%", typeof(SkiaSharp.SKSurface))
			) {
			VisualizerObjectSourceType = new(typeof(SkiaSharpVisualizerSource)),
			Style = VisualizerStyle.ToolWindow
		};

		/// <inheritdoc/>
		public override async Task<IRemoteUserControl> CreateVisualizerAsync(VisualizerTarget visualizerTarget, CancellationToken cancellationToken) {
			// The control will be in charge of calling the RequestDataAsync method from the visualizer object source and disposing of the visualizer target.
			return await Task.FromResult(new SkiaSharpVisualizerControl(visualizerTarget));
		}
	}

}
