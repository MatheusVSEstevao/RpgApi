using Microsoft.AspNetCore.Mvc;
using RpgApi.Data;
using RpgApi.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace RpgApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DisputasController : ControllerBase
    {
        //Construtores e métodos aqui.

        private readonly DataContext _context;
        public DisputasController(DataContext context)
        {
            _context = context;
        }
   
    }

[HttPost("Arma")]
public async Task<IActionResult> AtaquesComArmaAsync(Disputa d)
{
    try
    {
        //Programação dos próximos passos aqui
        Personagem atacante = await _context.Personagens
            .include(p => p.Arma)
            .FirstOrDefaultAsync(p => p.Id == d.AtacanteID);

        Personagem oponente = await _context.Personagens
            .FirstOrDefaultAsync(p => p.Id == d.OponenteId);

        int dano = atacante.Arma.Dano + (new Random().Next(atacante.Forca));

        dano = dano - new Random().Next(oponente.Defesa);

        if (dano > 0)
            oponente.PontosVida = oponente.PontosVida - (int)dano;
        if (oponente.PontosVida <= 0)
            d.Narracao = $"{oponente.Nome} foi derrotado!";

        _context.Personagens.Update(oponente);
        await _context.SaveChancesAsync();

        StringBuilder dados = new StringBuilder();
        dados.AppendFormat(" Atacante: {0}. ", atacante.Nome);
        dados.AppendFormat(" Oponente: {0} ", oponente.Nome);
        dados.AppendFormat(" Pontos de vida do atacante: {0}. ", atacante.PontosVida);
        dados.AppendFormat(" Pontos de vida do oponente: {0}. ", oponente.PontosVida);
        dados.AppendFormat(" Arma Utilizada: {0}. ", atacante.Arma.Nome);
        dados.AppendFormat(" Dano: {0}. ", dano);

        d.Narracao += dados.ToString();
        d.DataDisputa = DateTime.Now;
        _context.Disputas.Add(d);
        _context.SaveChanges();

        return Ok(d);
    }
    catch (System.Exception ex)
    {
        return BadRequest(ex.Message);
    }
}

[HttPost("Habilidade")]
public async Task<IActionResult> AtaqueComHabilidadeAsync(Disputa d)
{
    try
    {
        //Programação dos próximos passos vai aqui
        Personagem atacante = await _context.Personagens
            .include(p => p.PersonagemHabilidades)
            .ThenInclude(ph => ph.Habilidade)
            .FirstOrDefaultAsync(p => p.Id == d.OponenteId);

        Personagem oponente = await _context.Personagens
            .FirstOrDefaultAsync(p => p.Id == d.OponenteId);

        PersonagemHAbilidade ph = await _context.PersonagemHabilidades
            .include(p => p.Habilidade)
            .FirstOrDefaultAsync(phBusca => phBusca.HabilidadeId == d.HabilidadeId
            && phBusca.PersonagemId == d.AtacanteID);

        if (ph == null)
            d.Narracao = $"{atacante.Nome} não possui esta Habilidade";
        else
        {
            int dano = ph.Habilidade.Dano + (new Random().Next(atacante.Inteligencia));
            dano = dano new Random().Next(oponente.Defesa);

            if (dano > 0)
                oponente.PontosVida = oponente.PontosVida - dano;
            if (oponente.PontosVida <= 0)
                d.Narracao += $"{oponente.Nome} foi derrotado!";

            _context.Personagens.Update(oponente);
            await _context.SaveChancesAsync();

            StringBuilder dados = new StringBuilder();
            dados.AppendFormat(" Atacante: {0}. ", atacante.Nome);
            dados.AppendFormat(" Oponente: {0}. ", oponente.Nome);
            dados.AppendFormat(" Pontos de vida do atacante: {0}. ", atacante.PontosVida);
            dados.AppendFormat(" Pontos de vida do oponente: {0}. ", oponente.PontosVida);
            dados.AppendFormat(" Habilidade utilizada: {0}. ", ph.Habilidade.Nome);
            dados.AppendFormat(" Dano: {0}. ", dano);

            d.Narracao += dados.ToString();
            d.DataDisputa = DateTime.Now;
            _context.Disputas.Add(d);
            _context.SaveChanges();
        }

        return Ok(d);
    }
    catch (System.Exception ex)
    {
        return BadRequest(ex.Message);
    }
}

[HttpGet("PersonagemRandom")]
public async Task<IActionResult> Sorteio()
{
    List<Personagem> personagens =
        await _context.Personagens.ToListAsync();

    //Sorteio com numero da quantidade de personagens
    int sorteio = new Random().Next(personagens.Count);

    //busca na lista pelo indice sorteado (Não é o ID)
    Pesonagem p = personagens[sorteio];

    string msg =
        string.Format("Nº Sorteado {0}. Personagem: {1}", sorteio, p.Nome);
    
    return Ok(msg);
}


}