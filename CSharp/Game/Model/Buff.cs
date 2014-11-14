﻿using Common.Base;
using Common.Event;
using Common.Helper;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Model
{
    public class Buff: Entity<Buff>, IDisposable
	{
        [BsonElement]
        private int configId { get; set; }

		[BsonElement]
		private ObjectId ownerId;

        [BsonElement]
        private long expiration;

        [BsonIgnore]
        private ObjectId timerId;

        [BsonIgnore]
        public long Expiration 
        {
            get
            {
                return this.expiration;
            }
            set
            {
                this.expiration = value;
            }
        }

        [BsonIgnore]
        public ObjectId TimerId 
        {
            get
            {
                return this.timerId;
            }
            set
            {
                this.timerId = value;
            }
        }

        public Buff(int configId, ObjectId ownerId)
        {
            this.configId = configId;
			this.ownerId = ownerId;
            if (this.Config.Duration != 0)
            {
                this.Expiration = TimeHelper.Now() + this.Config.Duration;
            }

			if (this.Expiration != 0)
			{
				// 注册Timer回调
				Env env = new Env();
				env[EnvKey.OwnerId] = this.OwnerId;
				env[EnvKey.BuffId] = this.Id;
				this.TimerId = World.Instance.GetComponent<TimerComponent>()
						.Add(this.Expiration, CallbackType.BuffTimeoutCallback, env);
			}
        }

		public override void BeginInit()
		{
			base.BeginInit();
		}

		public override void EndInit()
		{
			base.EndInit();

			if (this.Expiration != 0)
			{
				// 注册Timer回调
				Env env = new Env();
				env[EnvKey.OwnerId] = this.OwnerId;
				env[EnvKey.BuffId] = this.Id;
				this.TimerId = World.Instance.GetComponent<TimerComponent>()
						.Add(this.Expiration, CallbackType.BuffTimeoutCallback, env);
			}
		}

		[BsonIgnore]
        public BuffConfig Config
        {
            get
            {
                return World.Instance.GetComponent<ConfigComponent>().Get<BuffConfig>(this.configId);
            }
        }

		[BsonIgnore]
		public ObjectId OwnerId
		{
			get
			{
				return ownerId;
			}

			set
			{
				this.ownerId = value;
			}
		}

		public void Dispose()
		{
			if (this.Expiration == 0)
			{
				return;
			}
			
			World.Instance.GetComponent<TimerComponent>().Remove(this.TimerId);
			this.expiration = 0;
		}
	}
}