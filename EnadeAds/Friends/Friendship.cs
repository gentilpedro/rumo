namespace EnadeAds.Friends;

/// <summary>
/// Link de amigo de uma conta: o endereço <c>/amigos/&lt;código&gt;</c> que a pessoa manda a quem quiser. É o único
/// jeito de chegar a ela sem um duelo em comum, porque não existe busca por nome nem por e-mail.
/// </summary>
public class FriendLink
{
    public required string UserId { get; set; }
    public required string Code { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}

/// <summary>
/// Pedido de amizade e, depois do aceite, a amizade. Enquanto <see cref="AcceptedAt"/> é nulo, é só um pedido:
/// quem pediu não vê nada da outra pessoa. Recusar, cancelar e desfazer apagam a linha.
/// </summary>
public class Friendship
{
    public int Id { get; set; }
    public required string RequesterId { get; set; }
    public required string AddresseeId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? AcceptedAt { get; set; }

    public bool IsAccepted => AcceptedAt is not null;

    public string OtherThan(string userId) => RequesterId == userId ? AddresseeId : RequesterId;
}

/// <summary>Bloqueio: a pessoa bloqueada não consegue pedir amizade a quem bloqueou, e não fica sabendo.</summary>
public class FriendBlock
{
    public int Id { get; set; }
    public required string BlockerId { get; set; }
    public required string BlockedId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
