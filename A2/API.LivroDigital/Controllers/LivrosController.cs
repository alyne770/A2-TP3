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
    public class LivrosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LivrosController(AppDbContext context)
        {
            _context = context;
        }


        /// <summary>
        /// Obtém todos os livros.
        /// </summary>
        /// <remarks>
        /// Este endpoint retorna uma lista de todos os livros disponíveis no banco de dados.
        /// </remarks>
        /// <returns>
        /// Uma lista de objetos <see cref="Livro"/>.
        /// Retorna o código de status 200 em caso de sucesso, ou 500 em caso de erro interno.
        /// </returns>
        /// <response code="200">Lista de livros retornada com sucesso.</response>
        /// <response code="500">Erro interno ao tentar processar a solicitação.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Livro>>> GetLivros()
        {
            try
            {
                // Carrega os livros com autores e categorias relacionados
                var livros = await _context.Livros
                    .Include(l => l.Autores) // Carrega os dados do Autor
                    .Include(l => l.Categorias) // Carrega os dados da Categoria
                    .ToListAsync();

                return Ok(livros);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro interno: {ex.Message}");
            }
        }


        /// <summary>
        /// Recupera um livro pelo identificador.
        /// </summary>
        /// <remarks>
        /// Este endpoint recupera um livro do banco de dados, passando o ID do livro.
        /// 
        /// ### Regras e Validações
        /// - O ID fornecido deve corresponder a um livro existente no banco de dados.
        /// - Caso o livro não seja encontrado, o método retornará o status **404 Not Found**.
        /// 
        /// </remarks>
        /// <param name="id">O identificador único do livro a ser recuperado.</param>
        /// <returns>O livro correspondente ao ID fornecido.</returns>
        /// <response code="200">O livro foi recuperado com sucesso.</response>
        /// <response code="404">O livro não foi encontrado.</response>
        /// <response code="500">Erro interno ao tentar recuperar o livro.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Livro>> GetLivro(int id)
        {
            try
            {
                // Busca o livro pelo ID
                var livro = await _context.Livros.FindAsync(id);

                if (livro == null)
                {
                    return NotFound("O livro com o ID especificado não foi encontrado.");
                }

                // Retorna o livro encontrado
                return Ok(livro);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro ao recuperar o livro: {ex.Message}");
            }
        }


        /// <summary>
        /// Atualiza um livro existente, incluindo seus autores e categorias.
        /// </summary>
        /// <remarks>
        /// Este endpoint permite atualizar um livro já existente no banco de dados, passando o ID do livro e os novos dados,
        /// incluindo autores e categorias associados ao livro.
        /// 
        /// ### Regras e Validações
        /// - O ID fornecido na URL deve corresponder ao ID do livro no corpo da requisição.
        /// - O livro deve existir no banco de dados para ser atualizado.
        /// - Os autores e categorias fornecidos devem ser validados no banco.
        /// - Caso o livro, autores ou categorias não sejam encontrados, o método retornará o status **404 Not Found**.
        /// - Caso ocorra um erro de concorrência, o método retornará um erro de **500 Internal Server Error**.
        /// </remarks>
        /// <param name="id">O identificador único do livro a ser atualizado.</param>
        /// <param name="livro">Os novos dados do livro a ser atualizado, incluindo autores e categorias.</param>
        /// <returns>Uma resposta indicando o sucesso ou falha na atualização do livro.</returns>
        /// <response code="204">O livro foi atualizado com sucesso.</response>
        /// <response code="400">O ID no caminho da URL não corresponde ao ID no corpo da requisição.</response>
        /// <response code="404">O livro, autor ou categoria com o ID fornecido não foi encontrado.</response>
        /// <response code="500">Erro interno ao tentar atualizar o livro.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutLivro(int id, Livro livro)
        {
            // Verifica se o ID fornecido na URL corresponde ao ID no corpo da requisição
            if (id != livro.LivroId)
            {
                return BadRequest("O ID fornecido na URL não corresponde ao ID do livro.");
            }

            // Verifica se o livro existe no banco
            var livroExistente = await _context.Livros
                .Include(l => l.Autores)  // Carrega os autores do livro
                .Include(l => l.Categorias)  // Carrega as categorias do livro
                .FirstOrDefaultAsync(l => l.LivroId == id);

            if (livroExistente == null)
            {
                return NotFound("O livro com o ID especificado não foi encontrado.");
            }

            // Atualiza o título, ano, etc., do livro
            livroExistente.Titulo = livro.Titulo;
            livroExistente.AnoPublicacao = livro.AnoPublicacao;
            livroExistente.EditoraId = livro.EditoraId;

            // Atualiza os autores do livro
            if (livro.Autores != null && livro.Autores.Any())
            {
                var autoresAtualizados = new List<Autor>();

                foreach (var autor in livro.Autores)
                {
                    if (autor.AutorId == 0)
                    {
                        // Adiciona um novo autor se o ID for 0
                        _context.Autores.Add(autor);
                        autoresAtualizados.Add(autor);
                    }
                    else
                    {
                        // Verifica se o autor já existe
                        var autorExistente = await _context.Autores
                            .FirstOrDefaultAsync(a => a.AutorId == autor.AutorId);

                        if (autorExistente != null)
                        {
                            autoresAtualizados.Add(autorExistente);
                        }
                        else
                        {
                            return NotFound($"Autor com ID {autor.AutorId} não encontrado ou está deletado.");
                        }
                    }
                }

                // Substitui a lista de autores do livro com a lista atualizada
                livroExistente.Autores = autoresAtualizados;
            }

            // Atualiza as categorias do livro
            if (livro.Categorias != null && livro.Categorias.Any())
            {
                var categoriasAtualizadas = new List<Categoria>();

                foreach (var categoria in livro.Categorias)
                {
                    if (categoria.CategoriaId == 0)
                    {
                        // Adiciona uma nova categoria se o ID for 0
                        _context.Categorias.Add(categoria);
                        categoriasAtualizadas.Add(categoria);
                    }
                    else
                    {
                        // Verifica se a categoria já existe
                        var categoriaExistente = await _context.Categorias
                            .FirstOrDefaultAsync(c => c.CategoriaId == categoria.CategoriaId);

                        if (categoriaExistente != null)
                        {
                            categoriasAtualizadas.Add(categoriaExistente);
                        }
                        else
                        {
                            return NotFound($"Categoria com ID {categoria.CategoriaId} não encontrada.");
                        }
                    }
                }

                // Substitui a lista de categorias do livro com a lista atualizada
                livroExistente.Categorias = categoriasAtualizadas;
            }

            // Marca o livro como modificado
            _context.Entry(livroExistente).State = EntityState.Modified;

            try
            {
                // Salva as mudanças no banco de dados
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LivroExists(id))
                {
                    return NotFound("O livro com o ID especificado não foi encontrado.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }




        /// <summary>
        /// Cria um novo livro.
        /// </summary>
        /// <remarks>
        /// Este endpoint cria um novo livro no sistema. O livro é adicionado ao banco de dados e, em seguida, o recurso criado é retornado com o código de status 201 (Created).
        /// A criação é feita a partir de um objeto de livro recebido no corpo da requisição.
        /// </remarks>
        /// <param name="livro">O livro a ser criado. Ele deve incluir todos os dados necessários para o cadastro, exceto o identificador (LivroId), que será gerado automaticamente.</param>
        /// <returns>
        /// Um objeto <see cref="Livro"/> representando o livro criado. O status de resposta será 201 (Created) e o recurso criado será retornado.
        /// </returns>
        /// <response code="201">Livro criado com sucesso.</response>
        /// <response code="400">Se a requisição for inválida, por exemplo, se os dados do livro não atenderem aos requisitos de validação.</response>
        /// <response code="500">Erro interno no servidor ao tentar processar a solicitação.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Livro>> PostLivro(Livro livro)
        {

            var editoraExiste = await _context.Editoras.AnyAsync(l => l.EditoraId == livro.EditoraId);
            if (!editoraExiste)
            {
                return BadRequest(new { error = "A editora informada não existe no banco de dados." });
            }

            var autoresFinal = new List<Autor>();
            if (livro.Autores != null && livro.Autores.Any())
            {
                foreach (var autor in livro.Autores)
                {
                    if (autor.AutorId == 0)
                    {                
                        _context.Autores.Add(autor);
                        autoresFinal.Add(autor);
                    }
                    else
                    {
                        var autorExistente = await _context.Autores
                            .FirstOrDefaultAsync(a => a.AutorId == autor.AutorId);

                        if (autorExistente == null)
                        {
                            return BadRequest($"O Autor com ID {autor.AutorId} não existe no banco de dados.");
                        }

                        autoresFinal.Add(autorExistente);
                    }
                }
            }

            var categoriasFinal = new List<Categoria>();
            if (livro.Categorias != null && livro.Categorias.Any())
            {
                foreach (var categoria in livro.Categorias)
                {
                    if (categoria.CategoriaId == 0)
                    {
                        _context.Categorias.Add(categoria);
                        categoriasFinal.Add(categoria);
                    }
                    else
                    {
                        var categoriaExistente = await _context.Categorias
                            .FirstOrDefaultAsync(c => c.CategoriaId == categoria.CategoriaId);

                        if (categoriaExistente == null)
                        {
                            return BadRequest($"A Categoria com ID {categoria.CategoriaId} não existe no banco de dados.");
                        }

                        categoriasFinal.Add(categoriaExistente);
                    }
                }
            }

            
            livro.Autores = autoresFinal;
            livro.Categorias = categoriasFinal;

            _context.Livros.Add(livro);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro ao salvar os dados: {ex.Message}");
            }

            return CreatedAtAction("GetLivro", new { id = livro.LivroId }, livro);
        }


        /// <summary>
        /// Exclui um livro.
        /// </summary>
        /// <remarks>
        /// Este endpoint exclui um livro passando o id.
        ///
        /// ### Regras e Validações
        /// - O livro deve existir no banco de dados.
        /// - O livro não pode ser excluído se estiver associado a um empréstimo.
        /// - Caso o livro não seja encontrado, o método retornará o status **404 Not Found**.
        /// 
        /// </remarks>
        /// <param name="id">O identificador único do livro a ser excluído.</param>
        /// <returns>Uma resposta indicando o resultado da exclusão.</returns>
        /// <response code="204">O livro foi excluído com sucesso.</response>
        /// <response code="404">O livro não foi encontrado.</response>
        /// <response code="400">O livro não pode ser excluído, pois está associado a um ou mais empréstimos.</response>
        /// <response code="500">Erro interno ao tentar excluir o livro.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteLivro(int id)
        {
            try
            {
                // Busca o livro pelo ID
                var livro = await _context.Livros.FindAsync(id);

                if (livro == null)
                {
                    return NotFound("O livro especificado não foi encontrado.");
                }

                // Verifica se o livro está associado a um empréstimo
                var emprestimosAssociados = await _context.Emprestimos
                    .Where(e => e.LivroId == id)
                    .ToListAsync();

                if (emprestimosAssociados.Any())
                {
                    return BadRequest("O livro não pode ser excluído, pois está associado a um ou mais empréstimos.");
                }

                // Remove o livro
                _context.Livros.Remove(livro);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Erro ao excluir o livro: {ex.Message}");
            }
        }



        private bool LivroExists(int id)
        {
            return _context.Livros.Any(e => e.LivroId == id);
        }
    }
}
