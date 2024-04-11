using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Extensibility;
using System.Reflection;

namespace SkiaSharpVisualizer {
	/// <summary>
	/// Extension entrypoint for the VisualStudio.Extensibility extension.
	/// </summary>
	[VisualStudioContribution]
	public class SkiaSharpVisualizerExtension : Extension {
		/// <inheritdoc/>
		public override ExtensionConfiguration ExtensionConfiguration => new() {
			Metadata = new(
					id: "SkiaSharpVisualizer.c3655891-53aa-416e-981c-17ea9a969e58",
					version: this.ExtensionAssemblyVersion,
					publisherName: "MapLarge",
					displayName: "SkiaSharp Visualizer",
					description: "Debugger visualizers for SkiaSharp images."),
		};

		/// <inheritdoc />
		protected override void InitializeServices(IServiceCollection serviceCollection) {
			base.InitializeServices(serviceCollection);

			// You can configure dependency injection here by adding services to the serviceCollection.
		}
	}
}
