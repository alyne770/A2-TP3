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
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext context)
        {
            _context = context;
        }


        /// <summary>
        /// Obtém todos os usuários.
        /// </summary>
        /// <remarks>
        /// Este endpoint retorna uma lista de todos os usuários cadastrados no banco de dados.
        /// </remarks>
        /// <returns>
        /// Uma lista de objetos <see cref="Usuario"/>.
        /// Retorna o código de status 200 em caso de sucesso, ou 500 em caso de erro interno.
        /// </returns>
        /// <response code="200">Lista de usuários retornada com sucesso.</response>
        /// <response code="500">Erro interno ao tentar processar a solicitação.</response>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> GetUsuarios()
        {
            try
            {
                return Ok(await _context.Usuarios.ToListAsync());
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro interno: {ex.Message}");
            }
        }


        /// <summary>
        /// Recupera um usuário pelo identificador.
        /// </summary>
        /// <remarks>
        /// Este endpoint recupera um usuário do banco de dados, passando o ID do usuário.
        /// 
        /// ### Regras e Validações
        /// - O ID fornecido deve corresponder a um usuário existente no banco de dados.
        /// - Caso o usuário não seja encontrado, o método retornará o status **404 Not Found**.
        /// 
        /// </remarks>
        /// <param name="id">O identificador único do usuário a ser recuperado.</param>
        /// <returns>O usuário correspondente ao ID fornecido.</returns>
        /// <response code="200">O usuário foi recuperado com sucesso.</response>
        /// <response code="404">O usuário não foi encontrado.</response>
        /// <response code="500">Erro interno ao tentar recuperar o usuário.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            try
            {
                // Busca o usuário pelo ID
                var usuario = await _context.Usuarios.FindAsync(id);

                if (usuario == null)
                {
                    return NotFound("O usuário com o ID especificado não foi encontrado.");
                }

                // Retorna o usuário encontrado
                return Ok(usuario);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro ao recuperar o usuário: {ex.Message}");
            }
        }


        /// <summary>
        /// Atualiza um usuário existente.
        /// </summary>
        /// <remarks>
        /// Este endpoint permite atualizar as informações de um usuário já existente no banco de dados, passando o ID do usuário e os novos dados.
        /// 
        /// ### Regras e Validações
        /// - O ID fornecido na URL deve corresponder ao ID do usuário no corpo da requisição.
        /// - O usuário deve existir no banco de dados para ser atualizado.
        /// - Caso o usuário não seja encontrado, o método retornará o status **404 Not Found**.
        /// - Se ocorrer um erro de concorrência (alterações simultâneas), o método retornará um erro de **500 Internal Server Error**.
        /// 
        /// </remarks>
        /// <param name="id">O identificador único do usuário a ser atualizado.</param>
        /// <param name="usuario">Os novos dados do usuário a ser atualizado.</param>
        /// <returns>Uma resposta indicando o sucesso ou falha na atualização do usuário.</returns>
        /// <response code="204">O usuário foi atualizado com sucesso.</response>
        /// <response code="400">O ID no caminho da URL não corresponde ao ID no corpo da requisição.</response>
        /// <response code="404">O usuário com o ID fornecido não foi encontrado.</response>
        /// <response code="500">Erro interno ao tentar atualizar o usuário.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutUsuario(int id, Usuario usuario)
        {
            // Verifica se o ID da URL corresponde ao ID no corpo da requisição
            if (id != usuario.UsuarioId)
            {
                return BadRequest("O ID fornecido na URL não corresponde ao ID do usuário.");
            }

            // Marca a entrada como modificada
            _context.Entry(usuario).State = EntityState.Modified;

            try
            {
                // Salva as mudanças no banco de dados
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Verifica se o usuário ainda existe no banco
                if (!UsuarioExists(id))
                {
                    return NotFound("O usuário com o ID especificado não foi encontrado.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }



        /// <summary>
        /// Cria um novo usuário.
        /// </summary>
        /// <remarks>
        /// Este endpoint cria um novo usuário no sistema. Antes de salvar o usuário no banco de dados, ele valida os dados recebidos. 
        /// Se os dados forem inválidos, o método retornará um erro de validação com o código de status 400 (Bad Request).
        /// Caso os dados sejam válidos, o usuário é salvo no banco de dados, e o recurso criado é retornado com o código de status 201 (Created).
        /// </remarks>
        /// <param name="usuario">
        /// O objeto <see cref="Usuario"/> que contém as informações do usuário a ser criado. Ele deve incluir todos os dados obrigatórios conforme as regras de validação.
        /// </param>
        /// <returns>
        /// Um objeto <see cref="Usuario"/> representando o usuário criado, juntamente com o código de status 201 (Created) se a criação for bem-sucedida.
        /// </returns>
        /// <response code="201">Usuário criado com sucesso.</response>
        /// <response code="400">
        /// Se os dados fornecidos forem inválidos ou incompletos, o método retorna um erro de validação detalhando os problemas encontrados.
        /// </response>
        /// <response code="500">Erro interno no servidor ao tentar processar a solicitação.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Usuario>> PostUsuario(Usuario usuario)
        {
            // Valida o modelo
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Retorna erros de validação
            }

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUsuario", new { id = usuario.UsuarioId }, usuario);
        }



        /// <summary>
        /// Exclui um usuário.
        /// </summary>
        /// <remarks>
        /// Este endpoint exclui um usuário passando o id.
        ///
        /// ### Regras e Validações
        /// - O usuário deve existir no banco de dados.
        /// - O usuário não pode ser excluído se estiver associado a um ou mais empréstimos.
        /// - Caso o usuário não seja encontrado, o método retornará o status **404 Not Found**.
        /// 
        /// </remarks>
        /// <param name="id">O identificador único do usuário a ser excluído.</param>
        /// <returns>Uma resposta indicando o resultado da exclusão.</returns>
        /// <response code="204">O usuário foi excluído com sucesso.</response>
        /// <response code="404">O usuário não foi encontrado.</response>
        /// <response code="400">O usuário não pode ser excluído, pois está associado a um ou mais empréstimos.</response>
        /// <response code="500">Erro interno ao tentar excluir o usuário.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            try
            {
                // Busca o usuário pelo ID
                var usuario = await _context.Usuarios.FindAsync(id);

                if (usuario == null)
                {
                    return NotFound("O usuário especificado não foi encontrado.");
                }

                // Verifica se o usuário está associado a um empréstimo
                var emprestimosAssociados = await _context.Emprestimos
                    .Where(e => e.UsuarioId == id)
                    .ToListAsync();

                if (emprestimosAssociados.Any())
                {
                    return BadRequest("O usuário não pode ser excluído, pois está associado a um ou mais empréstimos.");
                }

                // Remove o usuário
                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro ao excluir o usuário: {ex.Message}");
            }
        }


        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.UsuarioId == id);
        }
    }
}
