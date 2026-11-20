namespace EnadeAds.Friends;

/// <summary>O que as páginas dizem depois de um pedido de amizade.</summary>
public static class FriendMessages
{
    /// <summary>Código do aviso (<c>?aviso=</c>) quando o pedido deu certo ou já estava feito; nulo quando houve um problema.</summary>
    public static string? Notice(FriendRequestOutcome outcome) => outcome switch
    {
        FriendRequestOutcome.Requested => "pedido",
        FriendRequestOutcome.Accepted => "amigos",
        FriendRequestOutcome.AlreadyFriends => "ja-amigos",
        FriendRequestOutcome.AlreadyRequested => "ja-pedido",
        _ => null,
    };

    public static string Problem(FriendRequestOutcome outcome) => outcome switch
    {
        FriendRequestOutcome.Self => "Este é o seu próprio link. Mande para quem você quer ter como amigo.",
        FriendRequestOutcome.YouBlocked => "Você bloqueou essa pessoa. Desbloqueie em Amigos para poder pedir a amizade.",
        FriendRequestOutcome.TooManyFriends => $"Você já tem {FriendService.MaxFriends} amigos. Desfaça uma amizade para adicionar outra.",
        FriendRequestOutcome.TooManyRequests => $"Você tem {FriendService.MaxPendingSent} pedidos esperando resposta. Cancele algum para enviar outro.",
        _ => "Não foi possível enviar o pedido: o link pode ter sido trocado.",
    };
}
