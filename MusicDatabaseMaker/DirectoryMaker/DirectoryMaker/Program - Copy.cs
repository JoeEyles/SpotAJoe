/*using MetadataExtractor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace DirectoryMaker
{
    class Program
    {
        static string JsonFile = @"C:\Users\joe\Documents\TravelMap\Location_History_Joe.json";
        static string JsonOutputFile = @"C:\Users\joe\Documents\TravelMap\Website\js\locationHistory.js";
        static string ImageDirectory_Combined = @"C:\Users\joe\Documents\TravelMap\photos_CombinedAll";
        static string NewImageDirectory = @"C:\Users\joe\Documents\TravelMap\Website\images";
        static GoogleLocations googleLocations;
        static JoeLocations joeLocations = new JoeLocations();

        static void Main(string[] args)
        {
            Console.WriteLine("Starting Locations");
            LoadLocations();
            RemoveWrongTimeLocations();
            SortByTime();
            DeleteByTooMuchDistance();
            Console.WriteLine("Done Locations");
            Console.WriteLine("Starting Images");
            IterateThroughImages(ImageDirectory_Combined);
            Console.WriteLine("Done Camera images");
            Console.WriteLine("Done Images");
            Console.WriteLine("Writting locations...");
            WriteLocations();
            Console.WriteLine("Locations written");
            Console.ReadLine();
        }

        static void LoadLocations()
        {
            using (StreamReader r = new StreamReader(JsonFile))
            {
                Console.WriteLine("Loading Json");
                string json = r.ReadToEnd();
                Console.WriteLine("Json Read");
                googleLocations = JsonConvert.DeserializeObject<GoogleLocations>(json);
                Console.WriteLine("Json Serialised, there are " + googleLocations.locations.Length);
            }
        }

        static void WriteLocations()
        {
            Console.WriteLine("Writing JSON");
            string json = JsonConvert.SerializeObject(joeLocations);
            File.WriteAllText(JsonOutputFile, "var joeLocations = " + json);
            Console.WriteLine("Written JSON");
        }

        static void RemoveWrongTimeLocations()
        {
            Console.WriteLine("RemoveWrongTimeLocations");
            for (int i = 0; i < googleLocations.locations.Length; i++)
            {
                if (i % 10000 == 0)
                    Console.WriteLine("RemoveWrongTimeLocations: " + i + " of " + googleLocations.locations.Length);
                if (googleLocations.locations[i].timestampMs < 1537056000000 && googleLocations.locations[i].timestampMs > 1524528000000)
                {
                    if (googleLocations.locations[i].latitudeE7 < 510594670)
                    {
                        //if (googleLocations.locations[i].accuracy < 100)
                        //{
                            joeLocations.locations.Add(new SingleJoeLocation(googleLocations.locations[i].timestampMs, googleLocations.locations[i].longitudeE7, googleLocations.locations[i].latitudeE7));
                        //}
                    }
                }
            }
            Console.WriteLine("RemoveWrongTimeLocations done, there are now: " + joeLocations.locations.Count);
        }

        static void SortByTime()
        {
            Console.WriteLine("sorting by time...");
            joeLocations.locations.Sort((x, y) => x.timestampMs.CompareTo(y.timestampMs));
            Console.WriteLine("sorted by time");
        }

        static void DeleteByTooMuchDistance()
        {
            Console.WriteLine("DeleteByTooMuchDistance");

            JoeLocations tempLocations = new JoeLocations();
            SingleJoeLocation lastPostion = joeLocations.locations[0];
            int lastIndex = 1;
            double maxDistAddedNaturally = 0;
            for (int i = 1; i < joeLocations.locations.Count; i++)
            {
                if (i % 1000 == 0)
                    Console.WriteLine("deleting by distance... " + i);
                double dist = lastPostion.DistBetween(joeLocations.locations[i]);
                if (dist < 0.2)
                {
                    tempLocations.locations.Add(joeLocations.locations[i]);
                    lastPostion = joeLocations.locations[i];
                    lastIndex = i;
                    maxDistAddedNaturally = Math.Max(maxDistAddedNaturally, dist);
                }
                if (Math.Abs(i - lastIndex) > 100)//100
                {
                    i = lastIndex + 1;
                    lastPostion = joeLocations.locations[i];
                    tempLocations.locations.Add(joeLocations.locations[i]);
                }
            }
            joeLocations = tempLocations;
            Console.WriteLine("Deleted by distance with length: " + joeLocations.locations.Count);
            Console.WriteLine("With max dist: " + maxDistAddedNaturally);
        }

        static void IterateThroughImages(string imageDirectory)
        {
            string[] files = System.IO.Directory.GetFiles(imageDirectory);
            for (int i = 0; i < files.Length; i++)
            {
                if (i % 10 == 0)
                    Console.WriteLine("done images: " + i + " out of " + files.Length);
                IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata(files[i]);
                foreach (var directory in directories)
                {
                    if (directory.Name == "Exif IFD0")
                    {
                        try
                        {
                            SingleJoeLocation closestLoc = null;
                            foreach (Tag tag in directory.Tags)
                            {
                                if (tag.Name == "Date/Time")
                                {
                                    DateTime time = ParseDateTime(tag.Description);
                                    long unix = UnixTime(time);
                                    closestLoc = GetClosestJoeLocation(unix);
                                }
                            }
                            if (closestLoc != null)
                            {
                                string relativeDirectory = CopyImageAndGetrelativeFileName(files[i]);
                                closestLoc.images.Add(relativeDirectory);
                            }
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
            }
        }

        public static Char[] splitTime = new Char[] { ' ', ':' };
        public static DateTime ParseDateTime(string time)//2018:04:25 14:49:16
        {
            String[] vals = time.Split(splitTime, StringSplitOptions.RemoveEmptyEntries);
            DateTime toReturn = new DateTime(int.Parse(vals[0]), int.Parse(vals[1]), int.Parse(vals[2]), int.Parse(vals[3]), int.Parse(vals[4]), int.Parse(vals[5]));
            return toReturn;
        }

        public static string CopyImageAndGetrelativeFileName(string file)
        {
            string newDirectory = NewImageDirectory + @"\" + Path.GetFileName(file);
            string relativeDirectory = "images/" + Path.GetFileName(file);
            if (!File.Exists(newDirectory))
                File.Copy(file, newDirectory);
            return relativeDirectory;
        }

        public static SingleJoeLocation GetClosestJoeLocation(long timestampMs)
        {
            for (int i = 0; i < joeLocations.locations.Count - 1; i++)
            {
                if (timestampMs > joeLocations.locations[i].timestampMs && timestampMs < joeLocations.locations[i + 1].timestampMs)
                {
                    if (timestampMs - joeLocations.locations[i].timestampMs < joeLocations.locations[i + 1].timestampMs - timestampMs)
                        return joeLocations.locations[i];
                    else 
                        return joeLocations.locations[i+1];
                }
            }
            return null ;
        }

        public static long UnixTime(DateTime time)
        {
            var timeSpan = (time - new DateTime(1970, 1, 1, 0, 0, 0));
            return (long)timeSpan.TotalMilliseconds;
        }

        public static Char[] splitChars = new Char[] { ' ', '°', '\'', '\"' };
        public static double ParseLatLongGeo(Tag tag)
        {
            String[] vals = tag.Description.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
            double longLat = double.Parse(vals[0]) + double.Parse(vals[1]) / 60.0 + double.Parse(vals[2]) / 3600.0;
            return longLat;
        }
    }
}
*/