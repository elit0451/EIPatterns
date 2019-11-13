using System;
using System.Collections.Generic;

namespace Package
{
    public class PackageRepository
    {
        private Dictionary<string, List<Package>> packageRepo;
        private static PackageRepository _instance;
        public static PackageRepository Instance
        {
            get
            {
                if (_instance is null)
                    _instance = new PackageRepository();
                return _instance;
            }
        }

        private PackageRepository()
        {
            packageRepo = new Dictionary<string, List<Package>>();
            packageRepo.Add("CityCar", new List<Package>());
            packageRepo.Add("FamillyCar", new List<Package>());
            packageRepo.Add("Van", new List<Package>());

            packageRepo["CityCar"].Add(new Package(100, "Perfect for a busy city"));
            packageRepo["CityCar"].Add(new Package(200, "Great for 3 friends"));
            packageRepo["CityCar"].Add(new Package(300, "50% student discount"));
            packageRepo["FamillyCar"].Add(new Package(450, "Fast for a big family"));
            packageRepo["FamillyCar"].Add(new Package(470, "Fit all your groceries"));
            packageRepo["FamillyCar"].Add(new Package(599, "Kids ride for free"));
            packageRepo["Van"].Add(new Package(500, "No need to hire transporter"));
            packageRepo["Van"].Add(new Package(601, "1 extra dollar for greater set of km"));
            packageRepo["Van"].Add(new Package(900, "The good ol'mighty truck"));
        }

        public List<Package> GetPackagesByType(string carType)
        {
            return packageRepo[carType];
        }

        public Package GetPackageById(Guid guid)
        {
            foreach(KeyValuePair<string, List<Package>> pair in packageRepo)
                foreach(Package p in pair.Value)
                    if(p.Id == guid)
                        return p;
                
            return null;
        }
    }
}