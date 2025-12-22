namespace Crm.Application.Customers.Dtos;

public record CreateCustomerDto
(
    string IdentificationType,
    string IdentificationNumber,
    string FullName,
    string? DisplayName,
    DateOnly? BirthDate,
    IEnumerable<AddressDto>? Addresses,
    IEnumerable<ContactDto>? Contacts,
    IEnumerable<CustomerWorkInfoDto>? WorkInfos);