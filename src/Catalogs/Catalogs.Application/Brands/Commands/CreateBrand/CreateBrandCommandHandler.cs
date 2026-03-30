using Catalogs.Application.Abstractions.Messaging;
using Catalogs.Domain.Abstractions.Persistence;
using Catalogs.Domain.Brands;
using SharedKernel;
using SharedKernel.Contracts.Catalogs.Brands;

namespace Catalogs.Application.Brands.Commands.CreateBrand;

internal sealed class CreateBrandCommandHandler(
    IBrandRepository brandRepository,
    IUnitOfWork unitOfWork,
    IEventPublisher eventPublisher)
    : ICommandHandler<CreateBrandCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
    {
        if (await brandRepository.ExistsByNameAsync(request.Name, cancellationToken))
            return Result.Failure<Guid>(BrandError.NameAlreadyExists(request.Name));

        var result = ProductBrand.Create(request.Name, request.Description);

        if (result.IsFailure)
            return Result.Failure<Guid>(result.Error);

        var brand = result.Value;

        brandRepository.Add(brand);

        await eventPublisher.PublishAsync(new BrandCreatedMessage
        {
            BrandId = brand.Id,
            Name = brand.Name,
            Description = brand.Description,
            CreatedAt = brand.CreatedAt
        }, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(brand.Id);
    }
}
