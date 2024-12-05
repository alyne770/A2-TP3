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
    public class AutoresController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AutoresController(AppDbContext context)
        {
            _context = context;
        }


        /// <summary>
        /// Obtém todos os autores.
        /// </summary>
        /// <remarks>
        /// Este endpoint retorna uma lista de todos os autores disponíveis no banco de dados.
        /// </remarks>
        /// <returns>
        /// Uma lista de objetos <see cref="Autor"/>.
        /// Retorna o código de status 200 em caso de sucesso, ou 500 em caso de erro interno.
        /// </returns>
        /// <response code="200">Lista de autores retornada com sucesso.</response>
        /// <response code="500">Erro interno ao tentar processar a solicitação.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Autor>>> GetAutores()
        {
            try
            {
                return Ok(await _context.Autores.ToListAsync());
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro interno: {ex.Message}");
            }
        }


        /// <summary>
        /// Recupera um autor pelo identificador.
        /// </summary>
        /// <remarks>
        /// Este endpoint recupera um autor do banco de dados, passando o ID do autor.
        /// 
        /// ### Regras e Validações
        /// - O ID fornecido deve corresponder a um autor existente no banco de dados.
        /// - Caso o autor não seja encontrado, o método retornará o status **404 Not Found**.
        /// 
        /// </remarks>
        /// <param name="id">O identificador único do autor a ser recuperado.</param>
        /// <returns>O autor correspondente ao ID fornecido.</returns>
        /// <response code="200">O autor foi recuperado com sucesso.</response>
        /// <response code="404">O autor não foi encontrado.</response>
        /// <response code="500">Erro interno ao tentar recuperar o autor.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Autor>> GetAutor(int id)
        {
            try
            {
                // Busca o autor pelo ID
                var autor = await _context.Autores.FindAsync(id);

                if (autor == null)
                {
                    return NotFound("O autor com o ID especificado não foi encontrado.");
                }

                // Retorna o autor encontrado
                return Ok(autor);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro ao recuperar o autor: {ex.Message}");
            }
        }


        /// <summary>
        /// Atualiza um autor existente.
        /// </summary>
        /// <remarks>
        /// Este endpoint permite atualizar as informações de um autor já existente no banco de dados, passando o ID do autor e os novos dados.
        /// 
        /// ### Regras e Validações
        /// - O ID fornecido no caminho da URL deve corresponder ao ID do autor no corpo da requisição.
        /// - O autor deve existir no banco de dados para que seja atualizado.
        /// - Caso o autor não seja encontrado, o método retornará o status **404 Not Found**.
        /// - Se ocorrer um erro de concorrência (alterações simultâneas), o método retornará um erro de **500 Internal Server Error**.
        /// 
        /// </remarks>
        /// <param name="id">O identificador único do autor a ser atualizado.</param>
        /// <param name="autor">Os novos dados do autor a ser atualizado.</param>
        /// <returns>Uma resposta indicando o sucesso ou falha na atualização do autor.</returns>
        /// <response code="204">O autor foi atualizado com sucesso.</response>
        /// <response code="400">O ID no caminho da URL não corresponde ao ID no corpo da requisição.</response>
        /// <response code="404">O autor com o ID fornecido não foi encontrado.</response>
        /// <response code="500">Erro interno ao tentar atualizar o autor.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutAutor(int id, Autor autor)
        {
            // Verifica se o ID da URL corresponde ao ID no corpo da requisição
            if (id != autor.AutorId)
            {
                return BadRequest("O ID fornecido na URL não corresponde ao ID do autor.");
            }

            // Marca a entrada como modificada
            _context.Entry(autor).State = EntityState.Modified;

            try
            {
                // Salva as mudanças no banco de dados
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Verifica se o autor ainda existe no banco
                if (!AutorExists(id))
                {
                    return NotFound("O autor com o ID especificado não foi encontrado.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }



        /// <summary>
        /// Cria um novo autor.
        /// </summary>
        /// <remarks>
        /// Este endpoint cria um novo autor no banco de dados. O autor é adicionado à base de dados e, em seguida, o recurso criado é retornado com o código de status 201 (Created). 
        /// A criação é feita a partir de um objeto de autor recebido no corpo da requisição.
        /// </remarks>
        /// <param name="autor">O autor a ser criado. Ele deve incluir todos os dados necessários para o cadastro, exceto o identificador (AutorId), que será gerado automaticamente.</param>
        /// <returns>
        /// Um objeto <see cref="Autor"/> representando o autor criado. O status de resposta será 201 (Created) e o recurso criado será retornado.
        /// </returns>
        /// <response code="201">Autor criado com sucesso.</response>
        /// <response code="400">Se a requisição for inválida, por exemplo, se os dados do autor não atenderem aos requisitos de validação.</response>
        /// <response code="500">Erro interno no servidor ao tentar processar a solicitação.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Autor>> PostAutor(Autor autor)
        {
            _context.Autores.Add(autor);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetAutor", new { id = autor.AutorId }, autor);
        }


        /// <summary>
        /// Exclui um autor.
        /// </summary>
        /// <remarks>
        /// Este endpoint exclui um autor passando o id.
        ///
        /// ### Regras e Validações
        /// - O Autor deve existir no banco de dados e não pode já estar associado com um ou mais livros.
        /// - Caso o Autor não seja encontrado, o método retornará o status **404 Not Found**.
        /// 
        /// </remarks>
        /// <param name="id">O identificador único do autor a ser excluído.</param>
        /// <returns>Uma resposta indicando o resultado da exclusão.</returns>
        /// <response code="204">O Autor foi excluído com sucesso.</response>
        /// <response code="404">O Autor não foi encontrado.</response>
        /// <response code="400">O Autor está associado a um ou mais livros.</response>
        /// <response code="500">Erro interno ao tentar excluir o autor.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteAutor(int id)
        {
            try
            {
                // Busca o autor pelo ID
                var autor = await _context.Autores.FindAsync(id);

                if (autor == null)
                {
                    return NotFound("O autor especificado não foi encontrado.");
                }

                // Verifica se o autor está associado a algum livro
                var livrosAssociados = await _context.Livros
                    .Where(l => l.Autores.Any(a => a.AutorId == id))
                    .ToListAsync();

                if (livrosAssociados.Any())
                {
                    return BadRequest("O autor não pode ser excluído, pois está associado a um ou mais livros.");
                }

                // Remove o autor
                _context.Autores.Remove(autor);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro ao excluir o autor: {ex.Message}");
            }
        }


        private bool AutorExists(int id)
        {
            return _context.Autores.Any(e => e.AutorId == id);
        }
    }
}
