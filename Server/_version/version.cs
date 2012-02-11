namespace Vre.Server 
{ 
    internal static partial class VersionGen
    {
        public const string ProductName = "VR Estate server";
        public const string CompanyName = "3D Condo Explorer Corp.";
        public const string CopyrightString = "Copyright (C) 2010-2012 3D Condo Explorer Corp.";

        // C# compiler does not allow these to be integers.
        // Still, to conform to compiler's build process, keep these values integer only.
        public const string Major = "1";
        public const string Minor = "0";
        public const string Build = "4";
        public const string Revision = "1";

        public static string AssemblyVersionString = Major + "." + Minor + "." + Build + "." + Revision;
    }
}