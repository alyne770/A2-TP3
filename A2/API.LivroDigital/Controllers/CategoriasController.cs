using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.LivroDigital.Data;
using API.LivroDigital.Models;

namespace API.LivroDigital.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriasController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtém todas as categorias.
        /// </summary>
        /// <remarks>
        /// Este endpoint retorna uma lista de todas as categorias disponíveis no banco de dados.
        /// </remarks>
        /// <returns>
        /// Uma lista de objetos <see cref="Categoria"/>.
        /// Retorna o código de status 200 em caso de sucesso, ou 500 em caso de erro interno.
        /// </returns>
        /// <response code="200">Lista de categorias retornada com sucesso.</response>
        /// <response code="500">Erro interno ao tentar processar a solicitação.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Categoria>>> GetCategorias()
        {
            try
            {
                return Ok(await _context.Categorias.ToListAsync());
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro interno: {ex.Message}");
            }
        }


        /// <summary>
        /// Recupera uma categoria pelo identificador.
        /// </summary>
        /// <remarks>
        /// Este endpoint recupera uma categoria do banco de dados, passando o ID da categoria.
        /// 
        /// ### Regras e Validações
        /// - O ID fornecido deve corresponder a uma categoria existente no banco de dados.
        /// - Caso a categoria não seja encontrada, o método retornará o status **404 Not Found**.
        /// 
        /// </remarks>
        /// <param name="id">O identificador único da categoria a ser recuperada.</param>
        /// <returns>A categoria correspondente ao ID fornecido.</returns>
        /// <response code="200">A categoria foi recuperada com sucesso.</response>
        /// <response code="404">A categoria não foi encontrada.</response>
        /// <response code="500">Erro interno ao tentar recuperar a categoria.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Categoria>> GetCategoria(int id)
        {
            try
            {
                // Busca a categoria pelo ID
                var categoria = await _context.Categorias.FindAsync(id);

                if (categoria == null)
                {
                    return NotFound("A categoria com o ID especificado não foi encontrada.");
                }

                // Retorna a categoria encontrada
                return Ok(categoria);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro ao recuperar a categoria: {ex.Message}");
            }
        }


        /// <summary>
        /// Atualiza uma categoria existente.
        /// </summary>
        /// <remarks>
        /// Este endpoint permite atualizar as informações de uma categoria já existente no banco de dados, passando o ID da categoria e os novos dados.
        /// 
        /// ### Regras e Validações
        /// - O ID fornecido no caminho da URL deve corresponder ao ID da categoria no corpo da requisição.
        /// - A categoria deve existir no banco de dados para ser atualizada.
        /// - Caso a categoria não seja encontrada, o método retornará o status **404 Not Found**.
        /// - Se ocorrer um erro de concorrência (alterações simultâneas), o método retornará um erro de **500 Internal Server Error**.
        /// 
        /// </remarks>
        /// <param name="id">O identificador único da categoria a ser atualizada.</param>
        /// <param name="categoria">Os novos dados da categoria a ser atualizada.</param>
        /// <returns>Uma resposta indicando o sucesso ou falha na atualização da categoria.</returns>
        /// <response code="204">A categoria foi atualizada com sucesso.</response>
        /// <response code="400">O ID no caminho da URL não corresponde ao ID no corpo da requisição.</response>
        /// <response code="404">A categoria com o ID fornecido não foi encontrada.</response>
        /// <response code="500">Erro interno ao tentar atualizar a categoria.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutCategoria(int id, Categoria categoria)
        {
            // Verifica se o ID da URL corresponde ao ID no corpo da requisição
            if (id != categoria.CategoriaId)
            {
                return BadRequest("O ID fornecido na URL não corresponde ao ID da categoria.");
            }

            // Marca a entrada como modificada
            _context.Entry(categoria).State = EntityState.Modified;

            try
            {
                // Salva as mudanças no banco de dados
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Verifica se a categoria ainda existe no banco
                if (!CategoriaExists(id))
                {
                    return NotFound("A categoria com o ID especificado não foi encontrada.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }



        /// <summary>
        /// Cria uma nova categoria.
        /// </summary>
        /// <remarks>
        /// Este endpoint cria uma nova categoria no banco de dados. A categoria é adicionada à base de dados e, em seguida, o recurso criado é retornado com o código de status 201 (Created). 
        /// A criação é feita a partir de um objeto de categoria recebido no corpo da requisição.
        /// </remarks>
        /// <param name="categoria">A categoria a ser criada. Ela deve incluir todos os dados necessários para o cadastro, exceto o identificador (CategoriaId), que é gerado automaticamente.</param>
        /// <returns>
        /// Um objeto <see cref="Categoria"/> representando a categoria criada. O status de resposta será 201 (Created) e o recurso criado será retornado.
        /// </returns>
        /// <response code="201">Categoria criada com sucesso.</response>
        /// <response code="400">Se a requisição for inválida, por exemplo, se a categoria fornecida não atender aos requisitos de validação.</response>
        /// <response code="500">Erro interno no servidor ao tentar processar a solicitação.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Categoria>> PostCategoria(Categoria categoria)
        {
            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCategoria", new { id = categoria.CategoriaId }, categoria);
        }


        /// <summary>
        /// Exclui uma categoria.
        /// </summary>
        /// <remarks>
        /// Este endpoint exclui uma categoria passando o id.
        /// 
        /// ### Regras e Validações
        /// - A Categoria deve existir no banco de dados e não pode já estar associada a um ou mais livros.
        /// - Caso a Categoria não seja encontrada, o método retornará o status **404 Not Found**.
        /// 
        /// </remarks>
        /// <param name="id">O identificador único da categoria a ser excluída.</param>
        /// <returns>Uma resposta indicando o resultado da exclusão.</returns>
        /// <response code="204">A Categoria foi excluída com sucesso.</response>
        /// <response code="404">A Categoria não foi encontrada.</response>
        /// <response code="400">A Categoria está associada a um ou mais livros.</response>
        /// <response code="500">Erro interno ao tentar excluir a categoria.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteCategoria(int id)
        {
            try
            {
                // Busca a categoria pelo ID
                var categoria = await _context.Categorias.FindAsync(id);

                if (categoria == null)
                {
                    return NotFound("A categoria especificada não foi encontrada.");
                }

                // Verifica se a categoria está associada a algum livro
                var livrosAssociados = await _context.Livros
                    .Where(l => l.Categorias.Any(c => c.CategoriaId == id))
                    .ToListAsync();

                if (livrosAssociados.Any())
                {
                    return BadRequest("A categoria não pode ser excluída, pois está associada a um ou mais livros.");
                }

                // Remove a categoria
                _context.Categorias.Remove(categoria);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro ao excluir a categoria: {ex.Message}");
            }
        }


        private bool CategoriaExists(int id)
        {
            return _context.Categorias.Any(e => e.CategoriaId == id);
        }
    }
}
