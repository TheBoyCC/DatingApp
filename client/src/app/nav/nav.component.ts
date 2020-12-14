import { ChangeDetectionStrategy, Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Message } from '../_models/message';
import { AccountService } from '../_services/account.service';
import { MessageService } from '../_services/message.service';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit {
  model: any = {};
 
  constructor(public accountService: AccountService, private router: Router, private toastr: ToastrService) { }

  ngOnInit(): void {   
  }

  login() {
    this.accountService.login(this.model).subscribe(() => {  
      this.router.navigateByUrl("/members");          
    }, () => {
      this.toastr.error("Invalid credentials", "Login Failed");
    })
  }

  logout() {
    this.accountService.logout();
    this.router.navigateByUrl("/");
  }

  // loadMessages() {
  //   this.messageService.getMessages(1, 5, this.container).subscribe(response => {
  //     this.messages = response.result;   
  //     this.unreadMessages = this.messages.length;
  //   })
  // }
}
