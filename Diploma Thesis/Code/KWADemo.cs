private bool _dead;
private bool _seenPlayer;
private Vector2 _position;
private Player _player;
private int _minFollowDistance = 400;
public Rectangle Rectangle;
void Update(GameTime gameTime) {
    if (!_dead) {
        if (_player.IsAttacking && 
                    _player.PlayerAttackHitbox.Intersect(hitbox) &&
                    elapsedAttackedTime > attackedTimer) {
            TakeDamage(10);
        }
        var distanceToPlayer = 
            Vector2.Distance(_position, _player.Position);
        if (distanceToPlayer < _minFollowDistance || _seenPlayer) {
            var downTime = gameTime.ElapsedGameTime.TotalSeconds;
            if (Rectangle.Intersect(_player.Rectangle)) {
                Attack(_player);
            } else {
                var moveDir = _player.Position - _position;
                moveDir.Normalize();
                _position += moveDir * speed * downTime;
            }
        }
    }
}