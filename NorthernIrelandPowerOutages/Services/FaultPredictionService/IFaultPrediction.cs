using Domain.Backend;
using Domain.Frontend;

namespace FaultPredictionService
{
    public interface IFaultPrediction
    {
        Task<List<PredictionUI>?> GetFaultPredictions();
        Task<bool> Train(IEnumerable<OutagePredictionTrainingData> outagePredictionTrainingData);
    }
}