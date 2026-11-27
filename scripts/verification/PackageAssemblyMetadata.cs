using System.IO;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;

namespace AtomUI.PackageVerification
{
    // Read packaged bytes without loading a product assembly into the PowerShell process.
    /// <summary>
    /// Reads the identity of a packaged assembly from its bytes, without loading the assembly.
    /// </summary>
    public static class AssemblyMetadata
    {
        /// <summary>
        /// Copies the stream into memory and reads the assembly identity from the metadata of the resulting image.
        /// </summary>
        /// <returns>
        /// The simple assembly name, its version and the framework name of its
        /// <c>System.Runtime.Versioning.TargetFrameworkAttribute</c>, in that order.
        /// </returns>
        /// <exception cref="InvalidDataException">
        /// The <c>TargetFrameworkAttribute</c> is missing, appears more than once or has a malformed value.
        /// </exception>
        public static string[] Read(Stream input)
        {
            using (var bytes = new MemoryStream())
            {
                input.CopyTo(bytes);
                bytes.Position = 0;
                using (var pe = new PEReader(bytes))
                {
                    var reader = pe.GetMetadataReader();
                    var assembly = reader.GetAssemblyDefinition();
                    string framework = null;
                    foreach (var handle in assembly.GetCustomAttributes())
                    {
                        var attribute = reader.GetCustomAttribute(handle);
                        if (attribute.Constructor.Kind != HandleKind.MemberReference)
                        {
                            continue;
                        }
                        var member = reader.GetMemberReference((MemberReferenceHandle)attribute.Constructor);
                        if (member.Parent.Kind != HandleKind.TypeReference)
                        {
                            continue;
                        }
                        var type = reader.GetTypeReference((TypeReferenceHandle)member.Parent);
                        if (reader.GetString(type.Namespace) != "System.Runtime.Versioning" ||
                            reader.GetString(type.Name) != "TargetFrameworkAttribute")
                        {
                            continue;
                        }
                        var value = reader.GetBlobReader(attribute.Value);
                        if (value.ReadUInt16() != 1 || framework != null)
                        {
                            throw new InvalidDataException("Invalid or duplicate TargetFrameworkAttribute.");
                        }
                        framework = value.ReadSerializedString();
                    }
                    if (string.IsNullOrWhiteSpace(framework))
                    {
                        throw new InvalidDataException("Missing TargetFrameworkAttribute in package assembly.");
                    }
                    return new[] { reader.GetString(assembly.Name), assembly.Version.ToString(), framework };
                }
            }
        }
    }
}
