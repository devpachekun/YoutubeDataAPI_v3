using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

class Program
{
	static async Task Main(string[] args)
	{
		UserCredential credential;
		using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
		{
			credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
				GoogleClientSecrets.FromStream(stream).Secrets,
				new[] { YouTubeService.Scope.YoutubeForceSsl },
				"user",
				CancellationToken.None
			);
		}

		var youtubeService = new YouTubeService(new BaseClientService.Initializer()
		{
			HttpClientInitializer = credential,
			ApplicationName = "YouTube Comment Manager"
		});

		while (true)
		{
			Console.WriteLine("Seleccione una opción:");
			Console.WriteLine("1: Leer comentarios");
			Console.WriteLine("2: Publicar comentario");
			Console.WriteLine("3: Responder a un comentario");
			Console.WriteLine("0: Salir");
			var option = Console.ReadLine();

			switch (option)
			{
				case "1":
					await LeerComentarios(youtubeService);
					break;
				case "2":
					await PublicarComentario(youtubeService);
					break;
				case "3":
					await ResponderComentario(youtubeService);
					break;
				case "0":
					return;
				default:
					Console.WriteLine("Opción no válida.");
					break;
			}
		}
	}

	static async Task LeerComentarios(YouTubeService youtubeService)
	{
		var videoId = "VIDEOID"; // Id del video de referencia

		var commentRequest = youtubeService.CommentThreads.List("snippet");
		commentRequest.VideoId = videoId;
		commentRequest.MaxResults = 20; // máx cant. comentarios

		var commentResponse = await commentRequest.ExecuteAsync();

		Console.WriteLine("Comentarios del video:");
		foreach (var commentThread in commentResponse.Items)
		{
			var comment = commentThread.Snippet.TopLevelComment;
			Console.WriteLine($"Autor: {comment.Snippet.AuthorDisplayName}");
			Console.WriteLine($"Comentario: {comment.Snippet.TextOriginal}");
			Console.WriteLine($"ID del comentario: {comment.Id}");
			Console.WriteLine();
		}
	}

	static async Task PublicarComentario(YouTubeService youtubeService)
	{
		var videoId = "Pahn9p-VehI"; // Id del video de referencia
		Console.WriteLine("Ingrese el comentario a publicar:");
		var commentText = Console.ReadLine();

		var commentSnippet = new CommentSnippet
		{
			TextOriginal = commentText
		};

		var topLevelComment = new Comment
		{
			Snippet = commentSnippet
		};

		var commentThreadSnippet = new CommentThreadSnippet
		{
			VideoId = videoId,
			TopLevelComment = topLevelComment
		};

		var commentThread = new CommentThread
		{
			Snippet = commentThreadSnippet
		};

		var insertRequest = youtubeService.CommentThreads.Insert(commentThread, "snippet");
		var insertResponse = await insertRequest.ExecuteAsync();

		Console.WriteLine("Comentario publicado con éxito: " + insertResponse.Snippet.TopLevelComment.Snippet.TextOriginal);
	}

	static async Task ResponderComentario(YouTubeService youtubeService)
	{
		Console.WriteLine("Ingrese el ID del comentario al que desea responder:");
		var parentCommentId = Console.ReadLine();

		Console.WriteLine("Ingrese la respuesta:");
		var replyText = Console.ReadLine();

		var commentSnippet = new CommentSnippet
		{
			TextOriginal = replyText,
			ParentId = parentCommentId
		};

		var comment = new Comment
		{
			Snippet = commentSnippet
		};

		var insertRequest = youtubeService.Comments.Insert(comment, "snippet");
		var insertResponse = await insertRequest.ExecuteAsync();

		Console.WriteLine("Respuesta publicada con éxito: " + insertResponse.Snippet.TextOriginal);
	}
}
