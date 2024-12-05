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
    public class EditorasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EditorasController(AppDbContext context)
        {
            _context = context;
        }


        /// <summary>
        /// Obtém todas as editoras.
        /// </summary>
        /// <remarks>
        /// Este endpoint retorna uma lista de todas as editoras disponíveis no banco de dados.
        /// </remarks>
        /// <returns>
        /// Uma lista de objetos <see cref="Editora"/>.
        /// Retorna o código de status 200 em caso de sucesso, ou 500 em caso de erro interno.
        /// </returns>
        /// <response code="200">Lista de editoras retornada com sucesso.</response>
        /// <response code="500">Erro interno ao tentar processar a solicitação.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Editora>>> GetEditoras()
        {
            try
            {
                return Ok(await _context.Editoras.ToListAsync());
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro interno: {ex.Message}");
            }
        }


        /// <summary>
        /// Recupera uma editora pelo identificador.
        /// </summary>
        /// <remarks>
        /// Este endpoint recupera uma editora do banco de dados, passando o ID da editora.
        /// 
        /// ### Regras e Validações
        /// - O ID fornecido deve corresponder a uma editora existente no banco de dados.
        /// - Caso a editora não seja encontrada, o método retornará o status **404 Not Found**.
        /// 
        /// </remarks>
        /// <param name="id">O identificador único da editora a ser recuperada.</param>
        /// <returns>A editora correspondente ao ID fornecido.</returns>
        /// <response code="200">A editora foi recuperada com sucesso.</response>
        /// <response code="404">A editora não foi encontrada.</response>
        /// <response code="500">Erro interno ao tentar recuperar a editora.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Editora>> GetEditora(int id)
        {
            try
            {
                // Busca a editora pelo ID
                var editora = await _context.Editoras.FindAsync(id);

                if (editora == null)
                {
                    return NotFound("A editora com o ID especificado não foi encontrada.");
                }

                // Retorna a editora encontrada
                return Ok(editora);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro ao recuperar a editora: {ex.Message}");
            }
        }


        /// <summary>
        /// Atualiza uma editora existente.
        /// </summary>
        /// <remarks>
        /// Este endpoint permite atualizar as informações de uma editora já existente no banco de dados, passando o ID da editora e os novos dados.
        /// 
        /// ### Regras e Validações
        /// - O ID fornecido no caminho da URL deve corresponder ao ID da editora no corpo da requisição.
        /// - A editora deve existir no banco de dados para ser atualizada.
        /// - Caso a editora não seja encontrada, o método retornará o status **404 Not Found**.
        /// - Se ocorrer um erro de concorrência (alterações simultâneas), o método retornará um erro de **500 Internal Server Error**.
        /// 
        /// </remarks>
        /// <param name="id">O identificador único da editora a ser atualizada.</param>
        /// <param name="editora">Os novos dados da editora a ser atualizada.</param>
        /// <returns>Uma resposta indicando o sucesso ou falha na atualização da editora.</returns>
        /// <response code="204">A editora foi atualizada com sucesso.</response>
        /// <response code="400">O ID no caminho da URL não corresponde ao ID no corpo da requisição.</response>
        /// <response code="404">A editora com o ID fornecido não foi encontrada.</response>
        /// <response code="500">Erro interno ao tentar atualizar a editora.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutEditora(int id, Editora editora)
        {
            // Verifica se o ID da URL corresponde ao ID no corpo da requisição
            if (id != editora.EditoraId)
            {
                return BadRequest("O ID fornecido na URL não corresponde ao ID da editora.");
            }

            // Marca a entrada como modificada
            _context.Entry(editora).State = EntityState.Modified;

            try
            {
                // Salva as mudanças no banco de dados
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Verifica se a editora ainda existe no banco
                if (!EditoraExists(id))
                {
                    return NotFound("A editora com o ID especificado não foi encontrada.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        /// <summary>
        /// Cria uma nova editora.
        /// </summary>
        /// <remarks>
        /// Este endpoint cria uma nova editora no banco de dados. A editora é adicionada à base de dados e, em seguida, o recurso criado é retornado com o código de status 201 (Created). 
        /// A criação é feita a partir de um objeto de editora recebido no corpo da requisição.
        /// </remarks>
        /// <param name="editora">A editora a ser criada. Ela deve incluir todos os dados necessários para o cadastro, exceto o identificador (EditoraId), que é gerado automaticamente.</param>
        /// <returns>
        /// Um objeto <see cref="Editora"/> representando a editora criada. O status de resposta será 201 (Created) e o recurso criado será retornado.
        /// </returns>
        /// <response code="201">Editora criada com sucesso.</response>
        /// <response code="400">Se a requisição for inválida, por exemplo, se os dados da editora não atenderem aos requisitos de validação.</response>
        /// <response code="500">Erro interno no servidor ao tentar processar a solicitação.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Editora>> PostEditora(Editora editora)
        {
            _context.Editoras.Add(editora);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEditora", new { id = editora.EditoraId }, editora);
        }


        /// <summary>
        /// Exclui uma editora.
        /// </summary>
        /// <remarks>
        /// Este endpoint exclui uma editora passando o id.
        ///
        /// ### Regras e Validações
        /// - A editora deve existir no banco de dados e não pode já estar associada a um ou mais livros.
        /// - Caso a editora não seja encontrada, o método retornará o status **404 Not Found**.
        /// - Caso a editora esteja associada a livros, o método retornará o status **400 Bad Request**.
        /// 
        /// </remarks>
        /// <param name="id">O identificador único da editora a ser excluída.</param>
        /// <returns>Uma resposta indicando o resultado da exclusão.</returns>
        /// <response code="204">A editora foi excluída com sucesso.</response>
        /// <response code="404">A editora não foi encontrada.</response>
        /// <response code="400">A editora está associada a um ou mais livros.</response>
        /// <response code="500">Erro interno ao tentar excluir a editora.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteEditora(int id)
        {
            try
            {
                // Busca a editora pelo ID
                var editora = await _context.Editoras.FindAsync(id);

                if (editora == null)
                {
                    return NotFound("A editora especificada não foi encontrada.");
                }

                // Verifica se a editora está associada a algum livro
                var livrosAssociados = await _context.Livros
                    .Where(l => l.EditoraId == id)
                    .ToListAsync();

                if (livrosAssociados.Any())
                {
                    return BadRequest("A editora não pode ser excluída, pois está associada a um ou mais livros.");
                }

                // Remove a editora
                _context.Editoras.Remove(editora);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro ao excluir a editora: {ex.Message}");
            }
        }


        private bool EditoraExists(int id)
        {
            return _context.Editoras.Any(e => e.EditoraId == id);
        }
    }
}
