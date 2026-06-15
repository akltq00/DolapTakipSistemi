using DolapTakipSistemi.Application.Contracts;
using DolapTakipSistemi.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace DolapTakipSistemi.Api.Controllers;

[ApiController]
[Route("api/dolaplar")]
public class DolaplarController : ControllerBase
{
    private readonly IDolapService _dolapService;

    public DolaplarController(IDolapService dolapService)
    {
        _dolapService = dolapService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<DolapResponse>>> Listele(CancellationToken cancellationToken)
    {
        return Ok(await _dolapService.ListeleAsync(cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DolapResponse>> Getir(int id, CancellationToken cancellationToken)
    {
        var dolap = await _dolapService.GetirAsync(id, cancellationToken);

        if (dolap is null)
        {
            return NotFound();
        }

        return Ok(dolap);
    }

    [HttpPost]
    public async Task<ActionResult<DolapResponse>> Olustur(
        DolapOlusturRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var dolap = await _dolapService.OlusturAsync(request, cancellationToken);

            return CreatedAtAction(nameof(Getir), new { id = dolap.Id }, dolap);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(exception.Message);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<DolapResponse>> Guncelle(
        int id,
        DolapGuncelleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var dolap = await _dolapService.GuncelleAsync(id, request, cancellationToken);

            if (dolap is null)
            {
                return NotFound();
            }

            return Ok(dolap);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(exception.Message);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Sil(int id, CancellationToken cancellationToken)
    {
        var silindi = await _dolapService.SilAsync(id, cancellationToken);

        return silindi ? NoContent() : NotFound();
    }

    [HttpPost("{id:int}/zimmete-al")]
    public async Task<ActionResult<DolapResponse>> ZimmeteAl(
        int id,
        ZimmeteAlRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var dolap = await _dolapService.ZimmeteAlAsync(id, request, cancellationToken);

            if (dolap is null)
            {
                return NotFound();
            }

            return Ok(dolap);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(exception.Message);
        }
    }

    [HttpPost("{id:int}/zimmeti-kaldir")]
    public async Task<ActionResult<DolapResponse>> ZimmetiKaldir(
        int id,
        ZimmetiKaldirRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var dolap = await _dolapService.ZimmetiKaldirAsync(id, request, cancellationToken);

            if (dolap is null)
            {
                return NotFound();
            }

            return Ok(dolap);
        }
        catch (UnauthorizedAccessException exception)
        {
            return Unauthorized(exception.Message);
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(exception.Message);
        }
    }
}
