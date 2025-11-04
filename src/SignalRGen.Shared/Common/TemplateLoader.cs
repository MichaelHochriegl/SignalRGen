using System.Reflection;
using Scriban;

namespace SignalRGen.Generator.Common;

/// <summary>
/// Provides methods for loading and parsing embedded Scriban templates.
/// </summary>
internal static class TemplateLoader
{
    /// <summary>
    /// Retrieves an embedded Scriban template by its name, parses it, and returns the loaded template object.
    /// </summary>
    /// <param name="name">
    /// The name of the template to load. This should correspond to the resource name
    /// without the file extension, located in the 'SignalRGen.Generator.Sources' namespace.
    /// </param>
    /// <returns>
    /// A <see cref="Template"/> object that represents the parsed Scriban template.
    /// </returns>
    public static Template GetTemplate(string name)
    {
        var assemblyName = Assembly.GetExecutingAssembly().GetName();
        using var stream = Assembly
            .GetExecutingAssembly()
            .GetManifestResourceStream(
                $"{assemblyName.Name}.Sources.{name}.sbntxt"
            )!;

        using var reader = new StreamReader(stream);
        return Template.Parse(reader.ReadToEnd());
    }
}