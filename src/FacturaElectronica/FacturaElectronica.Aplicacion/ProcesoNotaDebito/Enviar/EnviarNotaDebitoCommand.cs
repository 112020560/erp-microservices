using FacturaElectronica.Aplicacion.Wrappers;
using MediatR;
using SharedKernel.Contracts.ElectronicInvoice;

namespace FacturaElectronica.Aplicacion.ProcesoNotaDebito.Enviar;

public record EnviarNotaDebitoCommand(ProcesoNotaDebitoRequest NotaDebito) : IRequest<ResultadoFacturaElectronica>;
