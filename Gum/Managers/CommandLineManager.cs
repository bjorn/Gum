﻿using Gum.DataTypes;
using Gum.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolsUtilities;

namespace Gum.Managers
{
    public class CommandLineManager : Singleton<CommandLineManager>
    {
        public string Glux
        {
            get;
            private set;
        }

        public string ElementName
        {
            get;
            private set;
        }



        public void ReadCommandLine()
        {
            string[] commandLineArgs = Environment.GetCommandLineArgs();
            GumCommands.Self.GuiCommands.PrintOutput(commandLineArgs.Length + " command line argument(s)...");

            foreach (var arg in commandLineArgs)
            {
                GumCommands.Self.GuiCommands.PrintOutput("Processing command line arg: " + arg);

                // For now we support a single command line argument, either the .glux or an
                // object within the .glux
                string argExtension = FileManager.GetExtension(arg);

                if (argExtension == "gumx")
                {
                    Glux = arg;
                }
                else if(argExtension == GumProjectSave.ComponentExtension ||
                    argExtension == GumProjectSave.ScreenExtension ||
                    argExtension == GumProjectSave.StandardExtension)
                {
                    ElementName = FileManager.RemovePath(FileManager.RemoveExtension(arg));

                    string gluxDirectory = FileManager.GetDirectory( FileManager.GetDirectory(arg));

                    Glux = System.IO.Directory.GetFiles(gluxDirectory)
                        .FirstOrDefault(item => item.ToLowerInvariant().EndsWith(".gumx"));
                }
            }
        }
    }
}
