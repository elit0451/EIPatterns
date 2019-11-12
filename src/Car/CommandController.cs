namespace Car
{
    public static class CommandController
    {
        public static string ExecuteCommand(string command)
        {
            switch (command)
            {
                case "fetchAllTypes":
                    return CarRepository.Instance.GetAllTypes();
                default:
                    return "No available command";
            }
        }
    }
}