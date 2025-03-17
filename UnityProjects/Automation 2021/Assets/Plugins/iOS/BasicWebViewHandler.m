#import <UIKit/UIKit.h>
#import "UnityAppController.h"

@interface BasicWebViewController : UIViewController<UIWebViewDelegate>
@property (nonatomic, strong) UIWebView *webView;
@property (nonatomic, strong) NSString *url;
@end

@implementation BasicWebViewController

- (void)viewDidLoad {
    [super viewDidLoad];
    self.webView = [[UIWebView alloc] initWithFrame:self.view.bounds];
    self.webView.delegate = self;
    [self.view addSubview:self.webView];

    UIButton *closeButton = [UIButton buttonWithType:UIButtonTypeSystem];
    closeButton.frame = CGRectMake(20, 40, 80, 30);
    [closeButton setTitle:@"Close" forState:UIControlStateNormal];
    [closeButton addTarget:self action:@selector(closeWebView) forControlEvents:UIControlEventTouchUpInside];
    [self.view addSubview:closeButton];

    [self.webView loadRequest:[NSURLRequest requestWithURL:[NSURL URLWithString:self.url]]];
}

- (void)closeWebView {
    [self dismissViewControllerAnimated:YES completion:nil];
}

@end

void _embrace_basic_open_web_view(const char *url) {
    NSString *urlString = [NSString stringWithUTF8String:"https://www.google.com"];
    BasicWebViewController *webViewController = [[BasicWebViewController alloc] init];
    webViewController.url = urlString;
    UnityAppController *appController = (UnityAppController *)[UIApplication sharedApplication].delegate;
    [appController.rootViewController presentViewController:webViewController animated:YES completion:nil];
}