using CSharpFunctionalExtensions;

namespace Logic.AppServices
{
    public interface ICommand
    {
    }

    public interface ICommandHandler<T>
        where T : ICommand
    {
        Result Handle(T command);
    }

}
