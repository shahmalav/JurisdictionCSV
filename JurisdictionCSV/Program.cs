using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JurisdictionCSV
{
    class Program
    {
        private static string sourceFilePath = @"/Source/File/Path";
        private static string destFilePath = @"/Destination/File/Path";

        static void Main(string[] args)
        {

            // Get Jurisdiction dump into object 
            var dump = CSVToObject(sourceFilePath);

            // get distinct jurisdictions 
            List<Jurisdiction> distinctJurisdictions = GetDistinctJurisdictionTable(dump).ToList();

            // add counties id to the distinct jurisdition
            var Jurisdictions = PopulateJurisdictionCoverageAreas(distinctJurisdictions, dump.ToList());

            // object to csv
            using (var sw = new StreamWriter(destFilePath))
            {
                sw.WriteLine(string.Format("{0},{1},{2}","Jurisdiction ID", "Name", "Coverage Areas ID"));
                foreach (var item in Jurisdictions)
                {
                    sw.WriteLine(String.Format("{0},{1},{2}", item.Id, item.Name, string.Join(";", item.CoverageArea)));
                }
            }

            Console.WriteLine("Conversion successful!");
            Console.ReadLine();
        }

        private static List<Jurisdiction> PopulateJurisdictionCoverageAreas(List<Jurisdiction> jurisdictions, List<JurisdictionDump> dump)
        {
            var data = from item in jurisdictions
                       select new Jurisdiction()
                       {
                           Id = item.Id,
                           Name = item.Name,
                           CoverageArea = dump.Where(x => x.Id == item.Id).Select(x => x.CountyId).ToList()
                       };

            return data.ToList();
        }

        private static List<Jurisdiction> GetDistinctJurisdictionTable(IEnumerable<JurisdictionDump> dump)
        {

            var data = from d in dump select new Jurisdiction() { Id = d.Id, Name = d.Name };

            return data.Distinct(new JurisdictionComparer()).ToList() ;
        }

        private static IEnumerable<JurisdictionDump> CSVToObject(string filePath)
        {
            try {
                var csvObject = from line in File.ReadAllLines(filePath).Skip(1)
                                let columns = line.Split(',')
                                select new JurisdictionDump() 
                                {                                    
                                    Name = columns[0],
                                    Id = int.Parse(columns[1]),
                                    StateId = int.Parse(columns[2]),
                                    County = columns[3],
                                    CountyId = int.Parse(columns[4])
                                };
                return csvObject;
            }
            catch(FileNotFoundException fnfex) { Console.WriteLine("Unable to load file, file not found." + fnfex.Message); }
            catch (Exception ex) { Console.WriteLine("Error occured while processing file" + ex.Message); }

            return null;
        }


    }

    public class JurisdictionDump
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FullName { get; set; }
        public string StatePub { get; set; }
        public int StateId { get; set; }
        public int CountyId { get; set; }
        public string CountyState { get; set; }
        public string County { get; set; }
    }


    public class Jurisdiction
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public List<int> CoverageArea { get; set; }

    }

    /// <summary>
    /// Comparer for Jurisdiction
    /// </summary>
    public class JurisdictionComparer : IEqualityComparer<Jurisdiction>
    {
        public bool Equals(Jurisdiction x, Jurisdiction y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.Id == y.Id && x.Name == y.Name;
        }

        public int GetHashCode(Jurisdiction obj)
        {            
            if (Object.ReferenceEquals(obj, null)) return 0;

            int hashProductName = obj.Name == null ? 0 : obj.Name.GetHashCode();

            int hashProductCode = obj.Id.GetHashCode();
           
            return hashProductName ^ hashProductCode;
        }
    }
}
