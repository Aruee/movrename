using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace movrename
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			Console.Write ("movrename settings: ");
			bool dryrun = false;
			bool verbose = false;
			int maxDiff = 5;
			string path = "";
			for (int i = 0; i < args.Length; i++) {
				if (args [i] == "-n") {
					dryrun = true;
					Console.Write ("Dry run, no changes to your files will happen; ");
				} else if (args [i] == "-v") {
					verbose = true;
					Console.Write ("Verbose output; ");
				} else if (args[i].StartsWith("-p")) {
					path = args [i].Substring (2);
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

			if (path == "") {
				Console.WriteLine ("Please provide a path using the -p parameter");
				return;
			}

			string[] filePaths = Directory.GetFiles(path, "MVI*.MOV");

			foreach (var file in filePaths) {
				if (verbose) {
					//Console.WriteLine ("Checking " + file);
				}
				string id = file.Substring (file.Length - 12).Replace ("MVI_", "").Replace (".MOV", "");
				int originalID = int.Parse (id);
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

					string[] filePathsA = Directory.GetFiles (path, "IMG_" + idA + ".JPG");
					string[] filePathsB = Directory.GetFiles (path, "IMG_" + idB + ".JPG");
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
						da = GetDate (new FileInfo (filePathsB [0]));
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
						string targetFilename = file.Replace ("MVI_", toUse.Year.ToString("D4") + "-" + toUse.Month.ToString("D2") + "-" + toUse.Day.ToString("D2") + "_MVI_");
						if (!dryrun) {
							File.Move (file, targetFilename);
						}
						if (verbose) {
							Console.WriteLine (file + " --> " + targetFilename);
						}
						break;
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
