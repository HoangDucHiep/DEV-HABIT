namespace DevHabit.Api.DTOs.Commom;

public interface ICollectionResponse<T>
{
    List<T> Items { get; init; }
}
