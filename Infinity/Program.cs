﻿using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using Infinity.Generators;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using PlanetaryProcessor;
using System.Net;
using System.Threading.Tasks;

namespace Infinity
{
    class Program
    {
        public static void Main(string[] args)
        {
			Console.Title = "Infinity Galaxy Generator";
            //====Things for the program itself====//
            //Uses american decimal system
            Thread.CurrentThread.CurrentCulture = Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            string userName = Environment.UserName;

            //Here are the wanted folders to work in
            string[] folders =
            {
                @"StarSystems",
                @"StarSystems/Cache",
                @"StarSystems/Stars",
                @"StarSystems/Planets",
                @"StarSystems/Wormholes",

                @"Planets",
                @"Planets/Moons",

                @"SharedData"
            };
            //=====================================//

            //====Some Variables====//
            string gameDataPath;
            int starNumber;
            double galaxySize;
            int galaxyType;
            int defaultGalaxyType = 1; //Spiral
            //======================//

            //====Loads static datas====//
            Dictionary<string, Dictionary<string, string>> starDatas = Datas.Old.Star.ComputeStarData();
            //==========================//

            //[Already] generates the seed

            //Takes infos from the user, return infos and seed
            Random random;

            int seed = UserEntry(defaultGalaxyType, out gameDataPath, out starNumber, out galaxySize, out galaxyType, out random, out bool wormholes);
            string KSPPath = Directory.GetParent(gameDataPath).ToString();
            gameDataPath += @"/Infinity/";
            Console.WriteLine(gameDataPath);

            //====Galaxy Settings====//
            Dictionary<string, double> galaxySettings = new Dictionary<string, double>();
            galaxySettings.Add("starNumber", starNumber);
            galaxySettings.Add("galaxySize", galaxySize);
            galaxySettings.Add("galaxyType", galaxyType);
            //=======================//

            FolderCheckingCreating(gameDataPath, folders);

            OldFilesDeleting(gameDataPath, folders);

            //Writes shared datas
            File.WriteAllText(gameDataPath + @"SharedData/StarCount.INFINITY", Convert.ToString(starNumber));
            File.WriteAllText(gameDataPath + @"SharedData/Seed.INFINITY", Convert.ToString(seed));

            //Loads templates
            Dictionary<string, string> templateFiles = TemplateLoader(gameDataPath);

            Console.WriteLine("Generating the galaxy..\n");
            PlanetaryProcessorDeleting();
            Galaxy.Generate(gameDataPath, galaxySettings, starDatas, random, templateFiles, wormholes);
            PlanetaryProcessorDeleting();

            Console.WriteLine("Galaxy generated. Have fun!\nThe application can automatically launch KSP for you if you would like. y/n.");
            while(true)
            {
                string input = Console.ReadLine();
                if(input == "y" || input == "Y" || input == "n" || input == "N")
                {

                }
                else
                {
                    Error("Please enter y or n!");
                }
            }

            //Exit function
            Console.WriteLine("Press any key to exit.");
            //Saving temporary shared datas
            
            Console.ReadKey();
        }

        /// <summary>
        /// Checks user's entries
        /// </summary>
        static int UserEntry(
            int defaultGalaxyType, out string gameDataPath, out int starNumber, out double galaxySize, out int galaxyType, out Random random, out bool wormholes)
        {
            //====Things for the program itself====//
            bool devMode = false;
            //=====================================//

            //====Placeholders====//
            gameDataPath = "Path To GameData";
            starNumber = 0;
            galaxySize = 0;
            galaxyType = 1;
            wormholes = false;
            int seed = 0;
            //random = new Random();
            //====================//

            //Checks for the developer mode
            if (File.Exists(@"C:/Infinity/Developer.INFINITY"))
            {
                devMode = true;
                gameDataPath = File.ReadAllText(@"C:/Infinity/Developer.INFINITY");
            }

            //Checks inputs
            try
            {
                //Checks for the GameData path
                if(!devMode)
                    Console.WriteLine("Welcome to Infinity, the procedural Galaxy generator!\n\nPlease enter your GameData folder path:");
                while (true)
                {
                    if (!devMode)
                    {
                        gameDataPath = Console.ReadLine();

                        if (File.Exists(gameDataPath + @"Squad/squadcore.ksp"))
                            break;

                        else
                        {
                            Error("GameData folder incorrect, retry with a correct one.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Congratulations! I have detected that you are a developer. Your GameData is located at: " + gameDataPath + ".");
                        Console.WriteLine("You have also bypassed the checks for a proper GameData. Live on the edge, but be careful.");
                        break;
                    }
                }

                //User's number of star input checking
                while (true)
                {
                    Console.WriteLine("How many stars do you want in your galaxy?\n\n" +
                        "(Recommended: 25 for a decent framerate)");

                    string input = Console.ReadLine();

                    if (Int32.TryParse(input, out starNumber)) break;

                    Error("Please enter an integer!");
                }

                //User's galaxy size input checking
                while (true)
                {
                    Console.WriteLine("\nEnter the radius of your Galaxy in Light-Years\n" +
                        "(Recommended: 0.5 Ly, and the max is what KSP can support, so you have to be careful with high values.");

                    string input = Console.ReadLine();

                    if (Double.TryParse(input, out galaxySize))
                        break;

                    Error("Please enter a number!");
                }     
                
                //Wormhole input checking
                Console.WriteLine("Woud you like wormholes in your galaxy? y/n or m for more information.");
                while(true)
                {
                    string input = Console.ReadLine();
                    if (input == "n" || input == "N")
                    {
                        wormholes = false;
                        break;
                    }
                    if (input == "y" || input == "Y")
                    {
                        wormholes = true;
                        break;
                    }
                    if (input == "m" || input == "M")
                    {
                        Console.WriteLine("Wormholes provide an easier method of transportation to other star systems. You need only unlock semi-advanced interplanetary exploration technology to use them. They require the Kopernicus Expansion, and are aimed toward making Infinity easier to visit without mods like KSPI-E or USI Warp Drives. Turn it off if you have an interstellar propulsion mod or enjoy a challenge. y/n.");
                    }
                }
                //User's advanced mode inputs
                while (true)
                {
                    Console.WriteLine("\nDo you want to access the advanced settings? (anything/n)");

                    string input = Console.ReadLine();

                    if (input.Equals("n") || input.Equals("N"))
                    {
                        galaxyType = defaultGalaxyType; //Spiral Galaxy
                        Random randomSeed = new Random(); seed = randomSeed.Next(int.MinValue, int.MaxValue);
                        break;
                    }
                    else
                    {
                        while (true)//Galaxy type choice
                        {
                            Console.WriteLine("Choose the type of galaxy:\n\n1 - Spiral (Default)\n2 - Elliptical\n");

                            input = Console.ReadLine();

                            if ((Int32.TryParse(input, out int inputInt) && (inputInt == 1 || inputInt == 2)))
                            {
                                galaxyType = inputInt;
                                break;
                            }

                            else
                            {
                                Error("Please enter a number!");
                            }
                        }

                        //Seed choice
                        Console.WriteLine("\nEnter a custom seed (Leave empty to use random)");

                        input = Console.ReadLine();

                        if (input.Equals("")) { Random randomSeed = new Random(); seed = randomSeed.Next(int.MinValue, int.MaxValue); }
                        else { seed = input.GetHashCode(); }

                        break;
                    }
                }

                //User's choice on delete/generation;
                while (true)
                {
                    Console.WriteLine("\nAre you sure to rebuild a whole new galaxy? The old one will be deleted and saves will be unusable (any key/n)");

                    if (Console.ReadLine().Equals("n"))
                    {
                        ExitFunction();
                    }
                    else
                    {
                        Console.WriteLine("Hang on a bit, the program is removing old files and creating new ones...");
                        break;
                    }
                }
            }

            catch (Exception e)
            {
                Error("Error during the process: " + e.ToString());
            }

            random = new Random(seed);
            return seed;
        }

        /// <summary>
        /// Checks if folder are missing
        /// </summary>
        static void FolderCheckingCreating(
            string gameDataPath, string[] folders)
        {
            //Detects if needed folder exits, and/or creates them if not
            try
            {
                for (int i = 0; i < folders.Length; i++)
                {
                    if (!Directory.Exists(gameDataPath + folders[i]))
                    {
                        Directory.CreateDirectory(gameDataPath + folders[i]);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Folder detection/creation failed: " + e.ToString());
            }
        }

        /// <summary>
        /// Checks for older files to delete
        /// </summary>
        static void OldFilesDeleting(
            string gameDataPath, string[] folders)
        {
            //Detects existing files and deleted them
            try
            {
                for (int i = 0; i < folders.Length; i++)
                {
                    string[] files = Directory.GetFiles(gameDataPath + folders[i]);

                    foreach (string file in files)
                    {
                        File.Delete(file);
                    }
                }
            }
            catch (Exception e)
            {
                Error("File detection/delete failed: " + e.ToString());
            }
        }

        /// <summary>
        /// Loads all template files
        /// </summary>
        static Dictionary<string, string> TemplateLoader(
            string gameDataPath)
        {
            //Detects and loads template files
            string[] files = Directory.GetFiles(gameDataPath + @"Templates");

            string loadedFile = null;

            Dictionary<string, string> templates = new Dictionary<string, string>();

            foreach (string file in files)
            {
                loadedFile = File.ReadAllText(file);
                templates.Add(Path.GetFileName(file).Replace(".cfg", null), loadedFile);
            }

            return templates;
        }

        /// <summary>
        /// Displays message when error happens
        /// </summary>
        static void Error(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        static void PlanetaryProcessorDeleting()
        {
            //Deletes old process and folder of planetaryprocessor
            try
            {
                Process[] prs = Process.GetProcesses();
                foreach (Process pr in prs)
                    if (pr.ProcessName == "PlanetaryProcessor.App")
                        pr.Kill();

                string dotplanetaryprocessor = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @".planetaryprocessor";
                if (File.Exists(dotplanetaryprocessor)) File.Delete(dotplanetaryprocessor);
            }
            catch
            {
                Error("Acces to process killing refused!");
            }
        }
        static void ExitFunction()
        {
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
            Environment.Exit(0);
        }
        static void LaunchKSP(string KSPPath, bool useLauncher)
        {
            
        }
    }
}