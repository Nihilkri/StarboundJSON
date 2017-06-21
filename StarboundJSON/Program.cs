using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json;


namespace StarboundJSON {
	class Program {
		static System.Collections.Specialized.StringCollection log = new System.Collections.Specialized.StringCollection();
		static string Root = "C:\\Steam\\steamapps\\common\\Starbound\\assets\\a\\";
		public struct Cont { public string fullpath, path, ali, desc; public int x, y, slot, nslot, frames; public double eff, neff, inc; }
		static public List<Cont> conts = new List<Cont>();
	
		static void Main(string[] args) {
			WalkDirectoryTree(new System.IO.DirectoryInfo(Root + "unpacked\\"), 0);
			WalkDirectoryTree(new System.IO.DirectoryInfo(Root + "es\\"), 1);
			WalkDirectoryTree(new System.IO.DirectoryInfo(Root + "esh\\"), 2);

			// Write out all the files that could not be processed.
			Console.WriteLine("Files with restricted access:");
			foreach(string s in log) {
				Console.WriteLine(s);
			}
			StreamWriter sw = File.CreateText(Root + "dump.txt");
			sw.WriteLine("Path, objectName, Description, X, Y, Slot, NSlot, Eff, NEff, Inc");
			foreach(Cont cont in conts) {
				sw.WriteLine("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}", cont.path, cont.ali, cont.desc, cont.x, cont.y, cont.slot, cont.nslot, cont.eff, cont.neff, cont.inc);
			} sw.Close();
			// Keep the console window open in debug mode.
			Console.WriteLine("Press any key");
			Console.ReadKey();


		} // Main
		static void WalkDirectoryTree(System.IO.DirectoryInfo root, int iter) {
			System.IO.FileInfo[] files = null;
			System.IO.DirectoryInfo[] subDirs = null;

			// First, process all the files directly under this folder
			try {
				files = root.GetFiles("*.objec" + ((iter > 0) ? "t.patch" : "t"));
			}
			// This is thrown if even one of the files requires permissions greater
			// than the application provides.
			catch(UnauthorizedAccessException e) {
				// This code just writes out the message and continues to recurse.
				// You may decide to do something different here. For example, you
				// can try to elevate your privileges and access the file again.
				log.Add(e.Message);
			} catch(System.IO.DirectoryNotFoundException e) {
				Console.WriteLine(e.Message);
			}
			Console.WriteLine("Directory: " + root.FullName);

			if(files != null) {
				foreach(System.IO.FileInfo fi in files) {
					// In this example, we only access the existing FileInfo object. If we
					// want to open, delete or modify the file, then
					// a try-catch block is required here to handle the case
					// where the file has been deleted since the call to TraverseTree().
					Cont cont;

					try {
						if(iter == 0) {
							{
								//DataContractJsonSerializer json = new DataContractJsonSerializer(File);
								dynamic json = JsonConvert.DeserializeObject(fi.OpenText().ReadToEnd());
								//Console.WriteLine(fi.FullName);
								cont = new Cont() { fullpath = fi.FullName, path = fi.Name.Substring(0, fi.Name.Length - 7), ali = json.objectName, desc = json.shortdescription };
								if(json.objectType != "container") continue;
								cont.slot = json.slotCount;
							}
							try {
								dynamic json = JsonConvert.DeserializeObject(new StreamReader(fi.FullName.Substring(0, fi.FullName.Length - 7) + ".frames").ReadToEnd());
								cont.x = (int)Math.Round((int)json.frameGrid.size[0] / 8.0); cont.y = (int)Math.Round((int)json.frameGrid.size[1] / 8.0);
							} catch(FileNotFoundException e) {
								try {
									dynamic json = JsonConvert.DeserializeObject(new StreamReader(fi.DirectoryName + "\\default.frames").ReadToEnd());
									cont.x = (int)Math.Round((int)json.frameGrid.size[0] / 8.0); cont.y = (int)Math.Round((int)json.frameGrid.size[1] / 8.0);
								} catch(Exception) {
									try {
										dynamic json = JsonConvert.DeserializeObject(new StreamReader(fi.FullName.Substring(0, fi.FullName.Length - 7) + "left.frames").ReadToEnd());
										cont.x = (int)Math.Round((int)json.frameGrid.size[0] / 8.0); cont.y = (int)Math.Round((int)json.frameGrid.size[1] / 8.0);
									} catch(Exception) {
										try {
											dynamic json = JsonConvert.DeserializeObject(new StreamReader(fi.FullName.Substring(0, fi.FullName.Length - 7) + "1.frames").ReadToEnd());
											cont.x = (int)Math.Round((int)json.frameGrid.size[0] / 8.0); cont.y = (int)Math.Round((int)json.frameGrid.size[1] / 8.0);
										} catch(Exception) {

											throw;
										} // 1
									} // left
								} // Default
							} // .feames
							cont.eff = cont.slot / (double)(cont.x * cont.y);
							Console.WriteLine("{0}, {1}, {2}, {3}, {4}, {5}", cont.ali, cont.desc, cont.x, cont.y, cont.slot, cont.eff);
							conts.Add(cont);
							//bm.Dispose();
						} // Iter == 0
						if(iter > 0) {
							{
								dynamic json = JsonConvert.DeserializeObject(fi.OpenText().ReadToEnd());
								int i = conts.FindIndex(x => x.ali == fi.Name.Substring(0, fi.Name.Length - 13));
								if(i == -1) {
									log.Add("Item not found!" + fi.FullName); //continue;
									cont = new Cont() { fullpath = fi.FullName, path = fi.Name.Substring(0, fi.Name.Length - 13) };
									try {
										dynamic pjson = JsonConvert.DeserializeObject(new StreamReader(Root + "unpacked\\" + fi.FullName.Substring(Root.Length + 2 + iter, fi.FullName.Length - (6 + Root.Length + 2 + iter))).ReadToEnd());
										cont.ali = pjson.objectName; cont.desc = pjson.shortdescription;
									} catch(DirectoryNotFoundException e) {
										continue;
									}	catch(Exception) {
										throw;
									} // unpacked.object
									try {
										dynamic pjson = JsonConvert.DeserializeObject(new StreamReader(Root + "unpacked\\" + fi.FullName.Substring(Root.Length + 2 + iter, fi.FullName.Length - (13 + Root.Length + 2 + iter)) + ".frames").ReadToEnd());
										cont.x = (int)Math.Round((int)pjson.frameGrid.size[0] / 8.0); cont.y = (int)Math.Round((int)pjson.frameGrid.size[1] / 8.0);
									} catch(Exception) {
										throw;
									} // unpacked.object

								} else cont = conts[i];
								foreach(dynamic js in json) {
									try {
										if(js.path == "/slotCount") cont.nslot = (int)js.value;
									} catch(Exception) {
										foreach(dynamic js2 in js) {
											try {
												if(js2.path == "/slotCount") cont.nslot = (int)js2.value;
											} catch(Exception) {
												throw;
												//continue;
											}
										}
										//continue;
									}
								} if(i == -1) { cont.slot = cont.nslot; cont.eff = cont.slot / (double)(cont.x * cont.y); }
								else if(cont.nslot == 0) cont.nslot = cont.slot; cont.neff = cont.nslot / (double)(cont.x * cont.y);
								cont.inc = cont.neff / cont.eff;
								if(i == -1) conts.Add(cont); else
									conts[i] = cont;
							}
							Console.WriteLine("{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}", cont.ali, cont.desc, cont.x, cont.y, cont.slot, cont.nslot, cont.eff, cont.neff, cont.inc);

						} // Iter > 0

					} catch(System.IO.FileNotFoundException e) {
						log.Add(e.Message);
					}

				} // ForEach

				// Now find all the subdirectories under this directory.
				subDirs = root.GetDirectories();

				foreach(System.IO.DirectoryInfo dirInfo in subDirs) {
					// Resursive call for each subdirectory.
					WalkDirectoryTree(dirInfo, iter);
				}
			}

		} // WalkDirectoryTree
	} // Class
} // Namespace
