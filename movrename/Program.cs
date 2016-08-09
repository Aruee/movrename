using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;
using System.Linq;

namespace movrename
{
	class MainClass
	{
		public static void Main (string[] args)
		{

			if (args.Length == 0) {
				Console.WriteLine ("movrename");
				Console.WriteLine ("Renames Canon DSLR movie files to contain the data they were taken on, based on photos that were taken before or after the video");
				Console.WriteLine ("Options are:");
				Console.WriteLine (" -v\t\tverbose output (default: off)");
				Console.WriteLine (" -n\t\tno changes / dry run (your files won't be altered) (default: off");
				Console.WriteLine (" -p\t\tthe path to look for files. This can be specified an arbitrary amount of times");
				Console.WriteLine (" -m\t\tthe maximum difference between the photo's and the movie's ID");
				Console.WriteLine ("");
				Console.WriteLine ("Please provide the proper options and run again.");
				return;
			}
			Console.Write ("movrename settings: ");
			bool dryrun = false;
			bool verbose = false;
			int maxDiff = 5;
			List<string> path = new List<string> ();
			for (int i = 0; i < args.Length; i++) {
				if (args [i] == "-n") {
					dryrun = true;
					Console.Write ("Dry run, no changes to your files will happen; ");
				} else if (args [i] == "-v") {
					verbose = true;
					Console.Write ("Verbose output; ");
				} else if (args[i].StartsWith("-p")) {
					path.Add(args [i].Substring (2));
					Console.Write ("Location " + path + "; ");
				} else if (args[i].StartsWith("-m")) {
					try {
						maxDiff = int.Parse(args[i].Substring(2));
						Console.Write("Maximum index difference set to " + maxDiff + "; ");
					} catch (Exception e) {
						Console.Write ("Maximum difference to target image provided but unparseable, sticking with default " + maxDiff + "; ");
					}
				}
			}
			Console.WriteLine ();

			if (path.Count == 0) {
				Console.WriteLine ("Please provide a path using the -p parameter");
				return;
			}

			List<string> filePaths = new List<string> ();
			foreach (var p in path) {
				string[] paths = Directory.GetFiles(p, "MVI*.MOV");
				filePaths.AddRange (paths);
			}

			foreach (var file in filePaths) {
				if (verbose) {
					//Console.WriteLine ("Checking " + file);
				}
				string id = file.Substring (file.Length - 12).Replace ("MVI_", "").Replace (".MOV", "");
				int originalID = int.Parse (id);
				bool success = false;
				for (int diff = 0; diff < maxDiff; diff++) {
					int a = originalID + diff;
					int b = originalID - diff;
					if (a > 9999) {
						a -= 10000;
					}
					if (b < 0) {
						b += 10000;
					}

					string idA = a.ToString ("D4");
					string idB = b.ToString ("D4");

					string[] filePathsA = path.SelectMany (p => Directory.GetFiles(p, "IMG_" + idA + ".JPG")).ToArray();// Directory.GetFiles (path, "IMG_" + idA + ".JPG");
					string[] filePathsB = path.SelectMany (p => Directory.GetFiles(p, "IMG_" + idB + ".JPG")).ToArray();
					DateTime da = new DateTime ();
					DateTime db = new DateTime ();
					bool useA = false;
					bool useB = false;
					
					DateTime baseline = new DateTime (0);

					//Console.WriteLine (filePathsA.Length + "/" + filePathsB.Length);

					if (filePathsA.Length == 1) {
						da = GetDate (new FileInfo (filePathsA [0]));
						if (da != baseline) {
							useA = true;
							if (verbose) {
								//Console.WriteLine ("Found date " + da + " in " + filePathsA [0]);
							}
						}
					}
					if (filePathsB.Length == 1) {
						db = GetDate (new FileInfo (filePathsB [0]));
						if (db != baseline) {
							useB = true;
							if (verbose) {
								//Console.WriteLine ("Found date " + db + " in " + filePathsB [0]);
							}
						}
					}

					DateTime toUse = new DateTime(0);
					bool perform = false;
					if (useA && !useB) {
						toUse = da;
						perform = true;
					} else if (!useA && useB) {
						toUse = db;
						perform = true;
					} else if (!useA && !useB) {
					} else {
						if ((da - baseline).TotalDays != (db - baseline).TotalDays) {
							Console.WriteLine ("Ambiguous dates found for " + file + ": " + da + " from " + filePathsA [0] + " and " + db + " from " + filePathsB [0] + ". Checking next file.");
							break;
						} else {
							toUse = da;
							perform = true;
						}
					}

					if (perform) {
						success = true;
						string targetFilename = file.Replace ("MVI_", toUse.Year.ToString ("D4") + "-" + toUse.Month.ToString ("D2") + "-" + toUse.Day.ToString ("D2") + "_MVI_");
						if (!dryrun) {
							File.Move (file, targetFilename);
						}
						if (verbose) {
							Console.WriteLine (file + " --> " + targetFilename);
						}
						break;
					}
				}
				if (!success) {	
					if (verbose) {
						Console.WriteLine ("Could not find appropriate match for " + file);
					}
				}
			}

		}

		public static DateTime GetDate(FileInfo f)
		{
			try {
				FileStream fs = new FileStream(f.FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
				Image img = Image.FromStream(fs);
				PropertyItem[] propItems = img.PropertyItems;
				foreach (var item in propItems) {
					//Console.WriteLine (item.Id.ToString ("X"));
					string id = item.Id.ToString ("X");
					if (id == "9003") {
						//Console.WriteLine ("Getting date... ");
						string value = System.Text.Encoding.ASCII.GetString (item.Value);
						//Console.WriteLine (value);
						int year = int.Parse (value.Substring (0, 4));
						int month = int.Parse (value.Substring (5, 2));
						int day = int.Parse (value.Substring (8, 2));
						DateTime result = new DateTime (year, month, day);
						//Console.WriteLine (result);
						return result;
					}
				}
			} catch {
				return (new DateTime (0));
			}
			return (new DateTime (0));
		}
	}
}
