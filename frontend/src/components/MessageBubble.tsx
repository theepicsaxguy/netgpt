import React from 'react';
import { Bot, User, Copy, ThumbsUp, ThumbsDown } from 'lucide-react';

interface MessageBubbleProps {
  role: string;
  content: string;
  timestamp?: string;
}

const MessageBubble: React.FC<MessageBubbleProps> = ({ role, content, timestamp }) => {
  const isUser = role.toLowerCase() === 'user';
  const isSystem = role.toLowerCase() === 'system';

  if (isSystem) {
    return (
        <div className="flex justify-center my-6">
            <div className="text-[10px] font-mono text-zinc-500 bg-zinc-900/50 border border-zinc-800/50 px-3 py-1 rounded">
                SYSTEM_EVENT: {content}
            </div>
        </div>
    );
  }

  return (
    <div className={`flex w-full group animate-in fade-in slide-in-from-bottom-2 duration-300 ${isUser ? 'justify-end' : 'justify-start'}`}>
      <div className={`flex max-w-[90%] md:max-w-[85%] lg:max-w-[75%] gap-4 ${isUser ? 'flex-row-reverse' : 'flex-row'}`}>
        
        {/* Avatar */}
        <div className={`flex-shrink-0 w-8 h-8 rounded flex items-center justify-center mt-1 border shadow-sm ${
          isUser 
            ? 'bg-zinc-100 border-zinc-200 text-zinc-900' 
            : 'bg-black border-zinc-800 text-zinc-400'
        }`}>
          {isUser ? <User size={16} strokeWidth={2.5} /> : <Bot size={16} strokeWidth={2.5} />}
        </div>

        {/* Content */}
        <div className={`flex flex-col ${isUser ? 'items-end' : 'items-start'} min-w-0 flex-1`}>
          <div className="flex items-center gap-3 mb-1.5 opacity-60">
            <span className={`text-xs font-semibold ${isUser ? 'text-zinc-300' : 'text-zinc-400'}`}>
                {isUser ? 'User' : 'NetGPT Agent'}
            </span>
            {timestamp && (
                <span className="text-[10px] font-mono text-zinc-600">
                {new Date(timestamp).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}
                </span>
            )}
          </div>
          
          <div className={`text-[15px] leading-7 whitespace-pre-wrap break-words ${
            isUser 
              ? 'bg-zinc-800/80 text-zinc-100 px-5 py-3 rounded-2xl rounded-tr-sm border border-zinc-700/50' 
              : 'bg-transparent text-zinc-300 px-0 py-0'
          }`}>
            {content}
          </div>

          {/* Action Bar */}
          {!isUser && (
              <div className="flex items-center gap-1 mt-3 opacity-0 group-hover:opacity-100 transition-opacity">
                <button className="p-1.5 text-zinc-600 hover:text-zinc-300 hover:bg-zinc-900 rounded transition-colors" title="Copy">
                    <Copy size={14} />
                </button>
                <div className="h-3 w-[1px] bg-zinc-800 mx-1"></div>
                <button className="p-1.5 text-zinc-600 hover:text-zinc-300 hover:bg-zinc-900 rounded transition-colors" title="Helpful">
                    <ThumbsUp size={14} />
                </button>
                <button className="p-1.5 text-zinc-600 hover:text-zinc-300 hover:bg-zinc-900 rounded transition-colors" title="Not Helpful">
                    <ThumbsDown size={14} />
                </button>
              </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default MessageBubble;