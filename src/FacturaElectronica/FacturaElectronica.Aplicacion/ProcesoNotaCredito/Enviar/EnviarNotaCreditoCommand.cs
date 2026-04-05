using FacturaElectronica.Aplicacion.Wrappers;
using MediatR;
using SharedKernel.Contracts.ElectronicInvoice;

namespace FacturaElectronica.Aplicacion.ProcesoNotaCredito.Enviar;

public record EnviarNotaCreditoCommand(ProcesoNotaCreditoRequest NotaCredito) : IRequest<ResultadoFacturaElectronica>;
