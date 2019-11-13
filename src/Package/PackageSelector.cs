using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Package
{
    public static class PackageSelector
    {
        public static JObject GetAvailablePackages(string carType)
        {
            JObject package = new JObject();
            List<Package> packagesToOffer = PackageRepository.Instance.GetPackagesByType(carType);

            package.Add("carType", carType);
            package.Add("packages", JsonConvert.SerializeObject(packagesToOffer));

            return package;
        }
    }
}