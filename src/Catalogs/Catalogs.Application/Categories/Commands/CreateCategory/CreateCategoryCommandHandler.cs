using Catalogs.Application.Abstractions.Messaging;
using Catalogs.Domain.Abstractions.Persistence;
using Catalogs.Domain.Categories;
using MassTransit;
using SharedKernel;
using SharedKernel.Contracts.Catalogs.Categories;

namespace Catalogs.Application.Categories.Commands.CreateCategory;

internal sealed class CreateCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    IPublishEndpoint eventPublisher)
    : ICommandHandler<CreateCategoryCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        if (await categoryRepository.ExistsByNameAsync(request.Name, cancellationToken))
            return Result.Failure<Guid>(CategoryError.NameAlreadyExists(request.Name));

        var result = ProductCategory.Create(request.Name, request.Description, request.ParentCategoryId);

        if (result.IsFailure)
            return Result.Failure<Guid>(result.Error);

        var category = result.Value;

        categoryRepository.Add(category);

        await eventPublisher.Publish(new CategoryCreatedMessage
        {
            CategoryId = category.Id,
            Name = category.Name,
            Description = category.Description,
            ParentCategoryId = category.ParentCategoryId,
            CreatedAt = category.CreatedAt
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(category.Id);
    }
}
