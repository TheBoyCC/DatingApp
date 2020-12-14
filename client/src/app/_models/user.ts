import { Message } from './message';


export interface User {
  username: string;
  token: string;
  photoUrl: string;
  knownAs: string;
  gender: string;
  roles: string[];
  messages: Message[];
}