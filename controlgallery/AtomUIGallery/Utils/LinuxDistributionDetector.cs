namespace AtomUIGallery.Utils;

using System;
using System.Collections.Generic;
using System.IO;

public static class LinuxDistributionDetector
{
    public static LinuxDistributionInfo DetectDistribution()
    {
        var info = new LinuxDistributionInfo();

        try
        {
            if (File.Exists("/etc/os-release"))
            {
                var lines     = File.ReadAllLines("/etc/os-release");
                var osRelease = ParseOsReleaseFile(lines);
                if (!string.IsNullOrEmpty(osRelease.Name))
                {
                    info.Name           = osRelease.Name;
                    info.Version        = osRelease.Version;
                    info.Id             = osRelease.Id;
                    info.DisplayVersion = osRelease.DisplayVersion;
                    info.PrettyName     = osRelease.PrettyName;
                    return info;
                }
            }

            info = CheckSpecificDistributionFiles();
            if (!string.IsNullOrEmpty(info.Name))
            {
                return info;
            }

            if (File.Exists("/etc/issue"))
            {
                var issueContent = File.ReadAllText("/etc/issue");
                info = ParseIssueFile(issueContent);
                if (!string.IsNullOrEmpty(info.Name))
                {
                    return info;
                }
            }

            info = GetLsbReleaseInfo();
            if (!string.IsNullOrEmpty(info.Name))
            {
                return info;
            }

            info.Name    = "Linux";
            info.Version = "Unknown";
        }
        catch (Exception ex)
        {
            info.Name    = "Linux";
            info.Version = $"Error: {ex.Message}";
        }

        return info;
    }

    private static LinuxDistributionInfo ParseOsReleaseFile(string[] lines)
    {
        var info       = new LinuxDistributionInfo();
        var properties = new Dictionary<string, string>();

        foreach (var line in lines)
        {
            var parts = line.Split('=', 2);
            if (parts.Length == 2)
            {
                var key   = parts[0].Trim();
                var value = parts[1].Trim().Trim('"');
                properties[key] = value;
            }
        }

        if (properties.TryGetValue("PRETTY_NAME", out var prettyName))
        {
            info.PrettyName = prettyName;
            info.Name       = prettyName;
        }
        else if (properties.TryGetValue("NAME", out var name))
        {
            info.Name = name;
        }

        if (properties.TryGetValue("VERSION", out var version))
        {
            info.Version        = version;
            info.DisplayVersion = version;
        }
        else if (properties.TryGetValue("VERSION_ID", out var versionId))
        {
            info.Version        = versionId;
            info.DisplayVersion = versionId;
        }

        if (properties.TryGetValue("ID", out var id))
        {
            info.Id = id;
        }

        if (string.IsNullOrEmpty(info.Name) && !string.IsNullOrEmpty(info.Id))
        {
            info.Name = GetDistributionNameFromId(info.Id);
        }

        return info;
    }

    private static LinuxDistributionInfo CheckSpecificDistributionFiles()
    {
        var info = new LinuxDistributionInfo();

        if (File.Exists("/etc/lsb-release"))
        {
            var lines = File.ReadAllLines("/etc/lsb-release");
            foreach (var line in lines)
            {
                if (line.StartsWith("DISTRIB_ID="))
                {
                    info.Name = line.Split('=')[1].Trim('"');
                }
                else if (line.StartsWith("DISTRIB_RELEASE="))
                {
                    info.Version = line.Split('=')[1].Trim('"');
                }
                else if (line.StartsWith("DISTRIB_DESCRIPTION="))
                {
                    info.PrettyName = line.Split('=')[1].Trim('"');
                }
            }

            if (!string.IsNullOrEmpty(info.Name))
            {
                return info;
            }
        }

        if (File.Exists("/etc/redhat-release"))
        {
            var content = File.ReadAllText("/etc/redhat-release").Trim();
            info.PrettyName = content;
            info.Name       = ParseRedHatRelease(content);
            info.Version    = ParseRedHatVersion(content);
            return info;
        }

        if (File.Exists("/etc/debian_version"))
        {
            var debianVersion = File.ReadAllText("/etc/debian_version").Trim();
            info.Name       = "Debian";
            info.Version    = debianVersion;
            info.PrettyName = $"Debian {debianVersion}";
            return info;
        }

        if (File.Exists("/etc/arch-release"))
        {
            info.Name       = "Arch Linux";
            info.PrettyName = "Arch Linux";
            return info;
        }

        if (File.Exists("/etc/alpine-release"))
        {
            var alpineVersion = File.ReadAllText("/etc/alpine-release").Trim();
            info.Name       = "Alpine Linux";
            info.Version    = alpineVersion;
            info.PrettyName = $"Alpine Linux {alpineVersion}";
            return info;
        }

        if (File.Exists("/etc/SuSE-release"))
        {
            var lines = File.ReadAllLines("/etc/SuSE-release");
            info.Name = "SUSE Linux";
            foreach (var line in lines)
            {
                if (line.StartsWith("VERSION ="))
                {
                    info.Version = line.Split('=')[1].Trim();
                }
                else if (line.StartsWith("PATCHLEVEL ="))
                {
                    info.Version += "." + line.Split('=')[1].Trim();
                }
            }
            info.PrettyName = $"SUSE Linux {info.Version}";
        }

        return info;
    }

    private static LinuxDistributionInfo ParseIssueFile(string content)
    {
        var info = new LinuxDistributionInfo();

        if (content.Contains("Ubuntu"))
        {
            info.Name       = "Ubuntu";
            info.PrettyName = "Ubuntu";
        }
        else if (content.Contains("Debian"))
        {
            info.Name       = "Debian";
            info.PrettyName = "Debian";
        }
        else if (content.Contains("CentOS"))
        {
            info.Name       = "CentOS";
            info.PrettyName = "CentOS";
        }
        else if (content.Contains("Red Hat"))
        {
            info.Name       = "Red Hat Enterprise Linux";
            info.PrettyName = "Red Hat Enterprise Linux";
        }
        else if (content.Contains("Fedora"))
        {
            info.Name       = "Fedora";
            info.PrettyName = "Fedora";
        }
        else if (content.Contains("Arch Linux"))
        {
            info.Name       = "Arch Linux";
            info.PrettyName = "Arch Linux";
        }
        else if (content.Contains("Alpine"))
        {
            info.Name       = "Alpine Linux";
            info.PrettyName = "Alpine Linux";
        }

        return info;
    }

    private static LinuxDistributionInfo GetLsbReleaseInfo()
    {
        var info = new LinuxDistributionInfo();

        try
        {
            using var process = new System.Diagnostics.Process();
            process.StartInfo.FileName               = "lsb_release";
            process.StartInfo.Arguments              = "-a";
            process.StartInfo.UseShellExecute        = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow         = true;

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            var lines = output.Split('\n');
            foreach (var line in lines)
            {
                if (line.StartsWith("Distributor ID:"))
                {
                    info.Name = line.Split(':')[1].Trim();
                }
                else if (line.StartsWith("Release:"))
                {
                    info.Version = line.Split(':')[1].Trim();
                }
                else if (line.StartsWith("Description:"))
                {
                    info.PrettyName = line.Split(':')[1].Trim();
                }
            }
        }
        catch
        {
            // lsb_release may not be available
        }

        return info;
    }

    private static string GetDistributionNameFromId(string id)
    {
        return id.ToLower() switch
        {
            "ubuntu"     => "Ubuntu",
            "debian"     => "Debian",
            "centos"     => "CentOS",
            "rhel"       => "Red Hat Enterprise Linux",
            "fedora"     => "Fedora",
            "arch"       => "Arch Linux",
            "alpine"     => "Alpine Linux",
            "opensuse"   => "openSUSE",
            "sles"       => "SUSE Linux Enterprise Server",
            "manjaro"    => "Manjaro Linux",
            "mint"       => "Linux Mint",
            "elementary" => "elementary OS",
            "pop"        => "Pop!_OS",
            "zorin"      => "Zorin OS",
            _            => id
        };
    }

    private static string ParseRedHatRelease(string content)
    {
        if (content.Contains("CentOS"))
        {
            return "CentOS";
        }

        if (content.Contains("Red Hat Enterprise Linux"))
        {
            return "Red Hat Enterprise Linux";
        }

        if (content.Contains("Fedora"))
        {
            return "Fedora";
        }

        if (content.Contains("AlmaLinux"))
        {
            return "AlmaLinux";
        }

        if (content.Contains("Rocky Linux"))
        {
            return "Rocky Linux";
        }

        return "Red Hat Based";
    }

    private static string ParseRedHatVersion(string content)
    {
        var parts = content.Split(' ');
        foreach (var part in parts)
        {
            if (Version.TryParse(part, out _))
            {
                return part;
            }
        }
        return "Unknown";
    }
}

public class LinuxDistributionInfo
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public string PrettyName { get; set; } = string.Empty;
    public string DisplayVersion { get; set; } = string.Empty;

    public override string ToString()
    {
        if (!string.IsNullOrEmpty(PrettyName))
        {
            return PrettyName;
        }

        if (!string.IsNullOrEmpty(Version))
        {
            return $"{Name} {Version}";
        }

        return Name;
    }
}
