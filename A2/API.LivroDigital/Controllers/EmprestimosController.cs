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
    public class EmprestimosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EmprestimosController(AppDbContext context)
        {
            _context = context;
        }


        /// <summary>
        /// Obtém todos os empréstimos.
        /// </summary>
        /// <remarks>
        /// Este endpoint retorna uma lista de todos os empréstimos registrados no banco de dados.
        /// </remarks>
        /// <returns>
        /// Uma lista de objetos <see cref="Emprestimo"/>.
        /// Retorna o código de status 200 em caso de sucesso, ou 500 em caso de erro interno.
        /// </returns>
        /// <response code="200">Lista de empréstimos retornada com sucesso.</response>
        /// <response code="500">Erro interno ao tentar processar a solicitação.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Emprestimo>>> GetEmprestimos()
        {
            try
            {
                return Ok(await _context.Emprestimos.ToListAsync());
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro interno: {ex.Message}");
            }
        }


        /// <summary>
        /// Recupera um empréstimo pelo identificador.
        /// </summary>
        /// <remarks>
        /// Este endpoint recupera um empréstimo do banco de dados, passando o ID do empréstimo.
        /// 
        /// ### Regras e Validações
        /// - O ID fornecido deve corresponder a um empréstimo existente no banco de dados.
        /// - Caso o empréstimo não seja encontrado, o método retornará o status **404 Not Found**.
        /// 
        /// </remarks>
        /// <param name="id">O identificador único do empréstimo a ser recuperado.</param>
        /// <returns>O empréstimo correspondente ao ID fornecido.</returns>
        /// <response code="200">O empréstimo foi recuperado com sucesso.</response>
        /// <response code="404">O empréstimo não foi encontrado.</response>
        /// <response code="500">Erro interno ao tentar recuperar o empréstimo.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Emprestimo>> GetEmprestimo(int id)
        {
            try
            {
                // Busca o empréstimo pelo ID
                var emprestimo = await _context.Emprestimos.FindAsync(id);

                if (emprestimo == null)
                {
                    return NotFound("O empréstimo com o ID especificado não foi encontrado.");
                }

                // Retorna o empréstimo encontrado
                return Ok(emprestimo);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro ao recuperar o empréstimo: {ex.Message}");
            }
        }


        /// <summary>
        /// Atualiza um empréstimo existente.
        /// </summary>
        /// <remarks>
        /// Este endpoint permite atualizar as informações de um empréstimo já existente no banco de dados, passando o ID do empréstimo e os novos dados.
        /// 
        /// ### Regras e Validações
        /// - O ID fornecido na URL deve corresponder ao ID do empréstimo no corpo da requisição.
        /// - O empréstimo deve existir no banco de dados para ser atualizado.
        /// - Caso o empréstimo não seja encontrado, o método retornará o status **404 Not Found**.
        /// - Se ocorrer um erro de concorrência (alterações simultâneas), o método retornará um erro de **500 Internal Server Error**.
        /// 
        /// </remarks>
        /// <param name="id">O identificador único do empréstimo a ser atualizado.</param>
        /// <param name="emprestimo">Os novos dados do empréstimo a ser atualizado.</param>
        /// <returns>Uma resposta indicando o sucesso ou falha na atualização do empréstimo.</returns>
        /// <response code="204">O empréstimo foi atualizado com sucesso.</response>
        /// <response code="400">O ID no caminho da URL não corresponde ao ID no corpo da requisição.</response>
        /// <response code="404">O empréstimo com o ID fornecido não foi encontrado.</response>
        /// <response code="500">Erro interno ao tentar atualizar o empréstimo.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutEmprestimo(int id, Emprestimo emprestimo)
        {
            // Verifica se o ID fornecido na URL corresponde ao ID no corpo da requisição
            if (id != emprestimo.EmprestimoId)
            {
                return BadRequest("O ID fornecido na URL não corresponde ao ID do empréstimo.");
            }

            // Marca a entrada como modificada
            _context.Entry(emprestimo).State = EntityState.Modified;

            try
            {
                // Salva as mudanças no banco de dados
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                // Verifica se o empréstimo ainda existe no banco
                if (!EmprestimoExists(id))
                {
                    return NotFound("O empréstimo com o ID especificado não foi encontrado.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }



        /// <summary>
        /// Cria um novo empréstimo.
        /// </summary>
        /// <remarks>
        /// Este endpoint cria um novo empréstimo no sistema. O empréstimo é adicionado ao banco de dados, mas antes de ser salvo, é feita uma validação para garantir que a 
        /// DataDevolucao não seja inferior à DataEmprestimo. Se essa condição não for atendida, será retornado um erro de validação.
        /// </remarks>
        /// <param name="emprestimo">O empréstimo a ser criado. Ele deve incluir todos os dados necessários para o cadastro, exceto o identificador (EmprestimoId), que será gerado automaticamente.</param>
        /// <returns>
        /// Um objeto <see cref="Emprestimo"/> representando o empréstimo criado. O status de resposta será 201 (Created) se a criação for bem-sucedida, ou 400 (Bad Request) se a validação falhar.
        /// </returns>
        /// <response code="201">Empréstimo criado com sucesso.</response>
        /// <response code="400">Se a data de devolução for inferior à data de empréstimo, ou se a requisição for inválida por outros motivos, como dados incorretos.</response>
        /// <response code="500">Erro interno no servidor ao tentar processar a solicitação.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Emprestimo>> PostEmprestimo(Emprestimo emprestimo)
        {
            
            var livroExiste = await _context.Livros.AnyAsync(l => l.LivroId == emprestimo.LivroId);
            if (!livroExiste)
            {
                return BadRequest(new { error = "O Livro informado não existe no banco de dados." });
            }

            var usuarioExiste = await _context.Usuarios.AnyAsync(l => l.UsuarioId == emprestimo.UsuarioId);
            if (!usuarioExiste)
            {
                return BadRequest(new { error = "O Usuario informado não existe no banco de dados." });
            }

            _context.Emprestimos.Add(emprestimo);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEmprestimo", new { id = emprestimo.EmprestimoId }, emprestimo);
        }



        /// <summary>
        /// Exclui um empréstimo.
        /// </summary>
        /// <remarks>
        /// Este endpoint exclui um empréstimo passando o id.
        ///
        /// ### Regras e Validações
        /// - O empréstimo deve existir no banco de dados.
        /// - Caso o empréstimo não seja encontrado, o método retornará o status **404 Not Found**.
        /// 
        /// </remarks>
        /// <param name="id">O identificador único do empréstimo a ser excluído.</param>
        /// <returns>Uma resposta indicando o resultado da exclusão.</returns>
        /// <response code="204">O empréstimo foi excluído com sucesso.</response>
        /// <response code="404">O empréstimo não foi encontrado.</response>
        /// <response code="500">Erro interno ao tentar excluir o empréstimo.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteEmprestimo(int id)
        {
            try
            {
                // Busca o empréstimo pelo ID
                var emprestimo = await _context.Emprestimos.FindAsync(id);

                if (emprestimo == null)
                {
                    return NotFound("O empréstimo especificado não foi encontrado.");
                }

                // Remove o empréstimo
                _context.Emprestimos.Remove(emprestimo);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro ao excluir o empréstimo: {ex.Message}");
            }
        }


        private bool EmprestimoExists(int id)
        {
            return _context.Emprestimos.Any(e => e.EmprestimoId == id);
        }
    }
}
