namespace Assets.Scripts
{
    class PacketType
    {
        public static readonly byte
            String = 0,
            PlayerJump = 2,
            PlayerTransformUpdate = 3,
            PlayerNameUpdate = 4,
            PlayerIdCodeUpdate = 5,
            StatusPlayerConnect = 6,
            StatusPlayerDisconnect = 7,
            PlayerHealthUpdate = 8,
            PlayerMaxHealthUpdate = 9,
            PlayerIdAssignment = 10,
            PlayerRegistration = 11,
            PlayerRegistrationResponse = 12,
            PlayerShootBullet = 13,
            PlayerControlsUpdate = 14;
    }
}
