using FacturaElectronica.Aplicacion.Wrappers;
using MediatR;
using SharedKernel.Contracts.ElectronicInvoice;

namespace FacturaElectronica.Aplicacion.ProcesoFactura.Enviar;

public record EnviarFacturaCommand(ProcesoFacturaRequest Factura): IRequest<ResultadoFacturaElectronica>;
