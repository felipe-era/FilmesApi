﻿using AutoMapper;
using FilmesAPI.Data;
using FilmesAPI.Data.Dtos;
using FilmesAPI.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace FilmesAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class FilmeController : ControllerBase
{
    #region == Atributos ==

    // \/ inicialmente utilizada para criar o bd
    //private static List<Filme> filmes = new List<Filme>();
    //private static int id = 0;

    private FilmeContext _context;
    private IMapper _mapper;

    public FilmeController(FilmeContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    #endregion
    /// <summary>
    /// Adiciona um filme ao banco de dados
    /// </summary>
    /// <param name="filmeDto">Objeto com os campos necessários para criação de um filme</param>
    /// <returns>IActionResult</returns>
    /// <response code="201">Caso inserção seja feita com sucesso</response>
    [HttpPost] //post
    [ProducesResponseType(StatusCodes.Status201Created)]
    public IActionResult AdicionaFilme([FromBody] CreateFilmeDto filmeDto) //[FromBody] é o que vem no corpo da requisição
    {
        Filme filme = _mapper.Map<Filme>(filmeDto);

        //Filme filme = new Filme(); fazendo a conversão manualmente (maneira incorreto), se usa o nuget
        //{
        //    filme.Titulo = filmeDto.Titulo;
        //}

        //por validações por region..
        //if ((!string.IsNullOrEmpty(filme.Titulo) && (!string.IsNullOrEmpty(filme.Genero)) && filme.Duracao >= 70))
        //{ } Não se utiliza validações aqui.. usa a data annotations la na classe filme no caso
        _context.Filmes.Add(filme);
        _context.SaveChanges();
        //Padrão rest como é?
        //R: deve-se retornar o objeto ao usuário (Retornar as informações que foram recém cadastradas (inseridas)
        //
        return CreatedAtAction(nameof(ConsultaFilmesPorId), //chama o método get para apresentar ao usuario
                               new { id = filme.Id }, //parametro do método acima
                               filme);
    }

    //[HttpGet] //consulta todos os filmes
    //public IEnumerable<Filme> ConsultaFilmes()
    //{
    //    return filmes;
    //}

    //[HttpGet("{id}")] //quando tiver parâmetros é necessario adicionar ao get
    //public Filme? ConsultaFilmesPorId(int id)
    //{
    //    return filmes.FirstOrDefault(filme => filme.Id == id);
    //    //o endereço informado pelo usuário retornar o endereço informado pelo usuário.
    //    //caso não tenha traz nulo
    //    //Ex: para trazer o filme id 1 usar o get no Postman/insomnia
    //    //https://localhost:7105/filme/1
    //}

    [HttpGet] //consulta filmes por intervalo skip and take 
    //https://localhost:7105/filme?skip=5&take=2 pula 5 e pega os 2 primeiros \/ quando não informado deixa o valor padrão como 0 ou 2
    public IEnumerable<ReadFilmeDto> ConsultaFilmesIntervalo([FromQuery] int skip = 0, [FromQuery] int take = 10)
    {
        return _mapper.Map<List<ReadFilmeDto>>(_context.Filmes.Skip(skip).Take(take));
    }

    [HttpGet("{id}")] //
    public IActionResult ConsultaFilmesPorId(int id)
    {
        var objfilme = _context.Filmes.FirstOrDefault(filme => filme.Id == id);
        if (objfilme == null) return NotFound();
        
        var filmeDto = _mapper.Map<ReadFilmeDto>(objfilme);
        return Ok(filmeDto);
    }

    [HttpPut("{id}")] //atualiza
    public IActionResult AtualizaFilme(int id, [FromBody]UpdateFilmeDto filmeDto)//frombody corpo da requisição
    {
        var filme = _context.Filmes.FirstOrDefault(filme => filme.Id == id); //uma variavel para o pegar o id
        if (filme == null) return NotFound();
        _mapper.Map(filmeDto, filme);//mapeamento dos campos filmeDto para o filme
        _context.SaveChanges();
        //retornar o status code (Atualização com sucesso)
        return NoContent(); //204
    }

    [HttpPatch("{id}")]//Atualiza parcialmente
    public IActionResult AtualizaFilmePatchParcial(int id, JsonPatchDocument<UpdateFilmeDto> patch )
    {
        var filme = _context.Filmes.FirstOrDefault(filme => filme.Id == id);
        if (filme == null) return NotFound();

        var filmeParaAtualizar = _mapper.Map<UpdateFilmeDto>(filme);
        patch.ApplyTo(filmeParaAtualizar, ModelState);

        if (!TryValidateModel(filmeParaAtualizar))//se não conseguir validar
        {
            return ValidationProblem(ModelState);
        }
        _mapper.Map(filmeParaAtualizar, filme);
        _context.SaveChanges();
        return NoContent(); //204
    }

    [HttpDelete("{id}")]
    public IActionResult DeletaFilme(int id)
    {
        var filme = _context.Filmes.FirstOrDefault(filme => filme.Id == id);
        if (filme == null) return NotFound();

        _context.Remove(filme);
        _context.SaveChanges(); 
        return NoContent(); //204 + atualizado ou removido com sucesso,

    }

}
