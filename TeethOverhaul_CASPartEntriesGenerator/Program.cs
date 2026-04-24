using System;
using System.Collections.Generic;
using System.IO;
using Mono.Cecil;
using s3pi.Interfaces;

namespace Destrospean.TeethOverhaul_CASPartEntriesGenerator
{
    class Program
    {
        public enum Locales
        {
            ENG_US,
            CHS_CN,
            CHT_CN,
            CZE_CZ,
            DAN_DK,
            DUT_NL,
            FIN_FI,
            FRE_FR,
            GER_DE,
            GRE_GR,
            HUN_HU,
            ITA_IT,
            JPN_JP,
            KOR_KR,
            NOR_NO,
            POL_PL,
            POR_PT,
            POR_BR,
            RUS_RU,
            SPA_ES,
            SPA_MX,
            SWE_SE,
            THA_TH
        }

        public static void Main(string[] args)
        {
            Console.Write("Specify a unique suffix for the batch of CASPart entries (leave blank for a random number suffix): ");

            // Get a unique name for the assembly and _XML resource
            string identifier = Console.ReadLine(),
            assemblyName = "TeethOverhaul_" + (string.IsNullOrEmpty(identifier) ? System.Security.Cryptography.FNV32.GetHash(Guid.NewGuid().ToString()).ToString() : identifier);

            // Load the base package and create a new package to clone to
            IPackage basePackage = s3pi.Package.Package.OpenPackage(0, "_TeethOverhaul_Base.package"),
            newPackage = s3pi.Package.Package.NewPackage(0);

            // Get the assembly and XML
            AssemblyDefinition assembly = null;
            var xmlDocument = new System.Xml.XmlDocument();
            xmlDocument.Load(args.Length == 0 ? "_TeethOverhaul_Base.xml" : args[0]);
            foreach (var resourceIndexEntry in basePackage.FindAll(x => x.Instance == System.Security.Cryptography.FNV64.GetHash("TeethOverhaul_Base")))
            {
                switch (resourceIndexEntry.ResourceType)
                {
                    case 0x73FAA07:
                        assembly = AssemblyDefinition.ReadAssembly(((ScriptResource.ScriptResource)s3pi.WrapperDealer.WrapperDealer.GetResource(0, basePackage, resourceIndexEntry)).Assembly.BaseStream);
                        break;
                }
            }

            // Return early if no assembly is found
            if (assembly == null)
            {
                return;
            }

            // Copy the elements from the XML to put into the new package
            System.Xml.XmlNode rootNode = xmlDocument.SelectSingleNode("Teeth");
            List<System.Xml.XmlElement> casPartElements = new List<System.Xml.XmlElement>(),
            categoryElements = new List<System.Xml.XmlElement>();
            foreach (System.Xml.XmlNode node in rootNode.ChildNodes)
            {
                if (node.Name == "CASPart" && !casPartElements.Exists(x => node.Attributes["Key"].Value == x.GetAttribute("Key")))
                {
                    casPartElements.Add((System.Xml.XmlElement)node);
                }
                if (node.Name == "Category" && !categoryElements.Exists(x => node.Attributes["ID"].Value == x.GetAttribute("ID")))
                {
                    categoryElements.Add((System.Xml.XmlElement)node);
                }
            }
            casPartElements.RemoveAt(0);
            categoryElements.RemoveAt(0);
            rootNode.RemoveAll();
            foreach (var casPartElement in casPartElements)
            {
                rootNode.AppendChild(casPartElement);
            }

            // Rename the assembly for the new package
            assembly.Name.Name = assemblyName;
            assembly.MainModule.Name = assemblyName + ".dll";

            Stream assemblyStream = new MemoryStream(),
            xmlStream = new MemoryStream();

            // Save the assembly with the new name
            assembly.Write(assemblyStream);

            // Save the new XML to a stream
            xmlDocument.Save(xmlStream);

            // Add the resources
            var s3saKeyInstance = System.Security.Cryptography.FNV64.GetHash(assemblyName);
            var nameMapResource = new NameMapResource.NameMapResource(0, null);
            var stblResources = new StblResource.StblResource[Enum.GetValues(typeof(Locales)).Length];
            for (var i = 0; i < stblResources.Length; i++)
            {
                var stblKeyInstance = ulong.Parse(i.ToString("X2") + s3saKeyInstance.ToString("X16").Substring(2), System.Globalization.NumberStyles.HexNumber);
                var stblName = "Strings_" + ((Locales)i).ToString() + "_0x" + stblKeyInstance.ToString("X16");
                stblResources[i] = new StblResource.StblResource(0, null);
                foreach (var categoryElement in categoryElements)
                {
                    stblResources[i].Add(System.Security.Cryptography.FNV64.GetHash("TeethOverhaul/TeethCategories:" + categoryElement.GetAttribute("ID")), categoryElement.GetAttribute("Path") ?? "");
                }
                foreach (var casPartElement in casPartElements)
                {
                    stblResources[i].Add(ulong.Parse(casPartElement.GetAttribute("Key").Substring(24), System.Globalization.NumberStyles.HexNumber), casPartElement.GetAttribute("Description") ?? "");
                }
                newPackage.AddResource(new ResourceKey(0x220557DA, 0, stblKeyInstance), stblResources[i].Stream, true);
                nameMapResource.Add(stblKeyInstance, stblName);
            }
            nameMapResource.Add(s3saKeyInstance, assemblyName);
            newPackage.AddResource(new ResourceKey(0x166038C, 0, 0), nameMapResource.Stream, true);
            newPackage.AddResource(new ResourceKey(0x333406C, 0, s3saKeyInstance), xmlStream, true);
            newPackage.AddResource(new ResourceKey(0x73FAA07, 0, s3saKeyInstance), new ScriptResource.ScriptResource(0, null)
                {
                    Assembly = new BinaryReader(assemblyStream)
                }.Stream, true);

            // Save the new package with the new name
            newPackage.SaveAs(assemblyName + ".package");
        }
    }
}
