namespace Service.Settings
{
    public class AppSettings
    {
        public ModelPrecisionSettings ModelPrecisionSettings { get; set; }

        public ModelAPISettings ModelAPISettings { get; set; }
    }

    public class ModelPrecisionSettings
    {
        public double ConfidenceTreshold { get; set; } = 0.96;
    }

    public class ModelAPISettings
    {
        public string Host { get; set; }

        public int Port { get; set; }

        public string PredictBatchImagesUrl { get; set; }

        public string PredictSingleImageUrl { get; set; }
    }
}
