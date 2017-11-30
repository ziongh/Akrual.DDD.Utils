using Akrual.DDD.Utils.Application.Messaging;

namespace Akrual.DDD.Utils.Application.UseCaseFunctions
{
    /// <summary>
    ///     Every application class should basically receive an input data from the presentation layer and return
    ///     an output to the presentation layer. So it should implement this interface
    /// </summary>
    /// <typeparam name="TInputModel">InputModel</typeparam>
    /// <typeparam name="TOutputModel">OutputModel</typeparam>
    public interface IUseCaseFunction<in TInputModel, out TOutputModel>
        where TInputModel : IInputModel
        where TOutputModel : IOutputModel
    {



        /// <summary>
        /// Every application class should basically receive an input data from the presentation layer and return
        /// an output to the presentation layer.
        /// </summary>
        /// <param name="input">Input model from the presentation layer</param>
        /// <returns>Output model to the presentation layer</returns>
        TOutputModel Execute(TInputModel input);
    }
}
